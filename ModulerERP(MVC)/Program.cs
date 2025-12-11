using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Models;
using ModulerERP_MVC_.Common.Behaviors;
using ModulerERP_MVC_.Common.Data;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Middleware;
using ModulerERP_MVC_.Common.Models;
using ModulerERP_MVC_.Common.Repositories.Interfaces;
using ModulerERP_MVC_.Data;
using ModulerERP_MVC_.Finance.Currencies.Repositories;
using ModulerERP_MVC_.Finance.Currencies.Services;
using ModulerERP_MVC_.Finance.ServieceForValidationAndmapping;
using ModulerERP_MVC_.Finance.Treasuries.Services;
using Serilog;
using System.Reflection;

namespace ModulerERP_MVC_
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // ============================================================================
            // SERILOG CONFIGURATION (Initialize Early)
            // ============================================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build())
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                    retainedFileCountLimit: 30)
                .CreateLogger();

            try
            {
                Log.Information("Starting ModularERP Multi-Tenant Application");

                var builder = WebApplication.CreateBuilder(args);

                // استخدام Serilog كـ Logger رئيسي
                builder.Host.UseSerilog();

                // ============================================
                // 1️⃣ Core Services
                // ============================================
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddMemoryCache();
                builder.Services.AddDistributedMemoryCache();

                // ============================================
                // 2️⃣ Master Database Context (للـ Tenants Management)
                // ============================================
                builder.Services.AddDbContext<MasterDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("MasterConnection"),
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly("ModulerERP(MVC)");
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        }));

                // ============================================
                // 3️⃣ Tenant Services
                // ============================================
                builder.Services.AddScoped<IMasterDbService, MasterDbService>();
                builder.Services.AddScoped<ITenantService, TenantService>();

                // ============================================
                // 4️⃣ Dynamic Tenant DbContext
                // ============================================
                builder.Services.AddScoped<ModulesDbContext>(provider =>
                {
                    var tenantService = provider.GetRequiredService<ITenantService>();
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    var logger = provider.GetRequiredService<ILogger<Program>>();

                    var tenantId = tenantService.GetCurrentTenantId();

                    string connectionString;
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        // استخدام Default Connection إذا مفيش Tenant
                        connectionString = configuration.GetConnectionString("DefaultConnection")!;
                        logger.LogWarning("No tenant context, using default connection");
                    }
                    else
                    {
                        connectionString = tenantService.GetConnectionString(tenantId);
                        logger.LogInformation("Using tenant connection for TenantId: {TenantId}", tenantId);
                    }

                    var optionsBuilder = new DbContextOptionsBuilder<ModulesDbContext>();
                    optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly("ModulerERP(MVC)");
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                    return new ModulesDbContext(optionsBuilder.Options);
                });

                // ============================================
                // 5️⃣ Repository Pattern
                // ============================================
                builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));
                builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();

                // 3. Register Services
                builder.Services.AddScoped<ICurrencyService, CurrencyService>();
                // ============================================
                // 6️⃣ Application Services
                // ============================================
                builder.Services.AddScoped<ITreasuryService, TreasuryService>();



                // Add MediatR Pipeline Behaviors for Validation
                builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                // ============================================
                // 9️⃣ FluentValidation
                // ============================================
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddFluentValidationClientsideAdapters();
                builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                // ============================================
                // 🔟 Management Services (من ServiceRegistration)
                // ============================================
                builder.Services.ManagementServices();

                // ============================================
                // 1️⃣1️⃣ Middleware
                // ============================================
                builder.Services.AddScoped<GlobalErrorHandlerMiddleware>();
                // AutoMapper
                builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

                // ============================================
                // 1️⃣2️⃣ MVC Configuration
                // ============================================
                builder.Services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    });

                // ============================================
                // 1️⃣3️⃣ Session Configuration
                // ============================================
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                var app = builder.Build();

                // ============================================
                // 1️⃣4️⃣ Initialize Master Database & Default Tenant
                // ============================================
                await EnsureMasterDatabaseAsync(app.Services);

                // ============================================
                // 1️⃣5️⃣ Configure HTTP Pipeline
                // ============================================
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                // ✅ Global Error Handler Middleware (يجب أن يكون أول middleware)
                app.UseMiddleware<GlobalErrorHandlerMiddleware>();

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();

                app.UseSession();

                // ============================================
                // 1️⃣6️⃣ Request Logging Middleware (Serilog)
                // ============================================
                app.UseSerilogRequestLogging(options =>
                {
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);

                        // إضافة Tenant ID للـ Logs
                        var tenantId = httpContext.Session.GetString("TenantId");
                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            diagnosticContext.Set("TenantId", tenantId);
                        }
                    };
                });

                // ============================================
                // 1️⃣7️⃣ Tenant Resolution Middleware (للـ MVC)
                // ============================================
                app.Use(async (context, next) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                    // Skip tenant check for static files and public paths
                    var path = context.Request.Path.Value?.ToLower();
                    if (path != null && (
                        path.StartsWith("/css/") ||
                        path.StartsWith("/js/") ||
                        path.StartsWith("/lib/") ||
                        path.StartsWith("/images/") ||
                        path.StartsWith("/favicon.ico") ||
                        path.StartsWith("/tenant/select")))
                    {
                        await next();
                        return;
                    }

                    var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                    var tenantId = tenantService.GetCurrentTenantId();

                    if (string.IsNullOrEmpty(tenantId))
                    {
                        // إذا مفيش Tenant في الـ Session، نعمل Redirect لصفحة اختيار الشركة
                        if (!context.Request.Path.StartsWithSegments("/Tenant"))
                        {
                            logger.LogWarning("No tenant context found, redirecting to tenant selection");
                            context.Response.Redirect("/Tenant/Select");
                            return;
                        }
                    }
                    else
                    {
                        // Validate Tenant
                        var isValid = await tenantService.ValidateTenantAsync(tenantId);
                        if (!isValid)
                        {
                            logger.LogWarning("Invalid tenant: {TenantId}", tenantId);
                            context.Session.Remove("TenantId");
                            context.Response.Redirect("/Tenant/Select");
                            return;
                        }

                        logger.LogInformation("Processing request for tenant: {TenantId}", tenantId);
                    }

                    await next();
                });

                app.UseAuthorization();

                // ============================================
                // 1️⃣8️⃣ Map Routes
                // ============================================
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                Log.Information("ModularERP application started successfully");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // ============================================
        // 🔧 Helper Method: Ensure Master DB & Default Tenant
        // ============================================
        private static async Task EnsureMasterDatabaseAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var masterDbService = scope.ServiceProvider.GetRequiredService<IMasterDbService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                Log.Information("Initializing Master Database");

                // إنشاء Master Database إذا مكانش موجود
                await masterContext.Database.EnsureCreatedAsync();
                await masterContext.Database.MigrateAsync();

                Log.Information("Master database initialized successfully");

                // إنشاء Default Tenant إذا مكانش موجود
                if (!await masterContext.MasterCompanies.AnyAsync())
                {
                    var defaultCompany = new MasterCompany
                    {
                        Id = Guid.NewGuid(),
                        Name = "Default Company",
                        CurrencyCode = "EGP",
                        DatabaseName = "ModularERP_Default",
                        Status = CompanyStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    masterContext.MasterCompanies.Add(defaultCompany);
                    await masterContext.SaveChangesAsync();

                    Log.Information("Created default company: {CompanyId} - {CompanyName}",
                        defaultCompany.Id, defaultCompany.Name);

                    // إنشاء قاعدة بيانات الـ Tenant
                    await masterDbService.CreateTenantDatabaseAsync(defaultCompany.Id);

                    Log.Information("Default tenant database created successfully");
                }
                else
                {
                    Log.Information("Master database already contains companies");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize master database");
                throw;
            }
        }
    }
}