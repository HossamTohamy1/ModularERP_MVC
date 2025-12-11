using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Models;
using ModulerERP_MVC_.Areas.Finance.Treasuries.Services;
using ModulerERP_MVC_.Common.Data;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Models;
using ModulerERP_MVC_.Modules.Data;

namespace ModulerERP_MVC_
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            // 5️⃣ Register Application Services
            // ============================================
            builder.Services.AddScoped<ITreasuryService, TreasuryService>();

            // ============================================
            // 6️⃣ MVC & Session
            // ============================================
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // ============================================
            // 7️⃣ Initialize Master Database & Default Tenant
            // ============================================
            await EnsureMasterDatabaseAsync(app.Services);

            // ============================================
            // 8️⃣ Configure HTTP Pipeline
            // ============================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            // ============================================
            // 9️⃣ Tenant Resolution Middleware (للـ MVC)
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
            // 🔟 Map Routes
            // ============================================
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
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
                // إنشاء Master Database إذا مكانش موجود
                await masterContext.Database.EnsureCreatedAsync();
                await masterContext.Database.MigrateAsync();

                logger.LogInformation("Master database initialized successfully");

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

                    logger.LogInformation("Created default company: {CompanyId}", defaultCompany.Id);

                    // إنشاء قاعدة بيانات الـ Tenant
                    await masterDbService.CreateTenantDatabaseAsync(defaultCompany.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize master database");
                throw;
            }
        }
    }
}