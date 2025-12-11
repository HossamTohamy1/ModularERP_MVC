using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Areas.Finance.Services;
using ModulerERP_MVC_.Modules.Data;

namespace ModulerERP_MVC_
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext with SQL Server
            builder.Services.AddDbContext<ModulesDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly("ModulerERP(MVC)");
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Register Services
            builder.Services.AddScoped<ITreasuryService, TreasuryService>();

            // Add MVC services with views
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            // Add Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add Memory Cache
            builder.Services.AddMemoryCache();



            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add Distributed Memory Cache
            builder.Services.AddDistributedMemoryCache();

            var app = builder.Build();

            // Apply Migrations Only
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ModulesDbContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Applying database migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // Configure the HTTP request pipeline
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

            // Map Routes - Areas first
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default Route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
    }
}