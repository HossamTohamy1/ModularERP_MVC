using ModulerERP_MVC_.Finance.Company.Repositories;
using ModulerERP_MVC_.Finance.Company.Services;
using ModulerERP_MVC_.Finance.Currencies.Repositories;
using ModulerERP_MVC_.Finance.Currencies.Services;
using ModulerERP_MVC_.Finance.Treasuries.Services;
using System.Reflection;

namespace ModulerERP_MVC_.Finance.ServieceForValidationAndmapping
{
    public static class ServiceRegistration
    {
        public static IServiceCollection ManagementServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // ✅ Register MediatR handlers ONCE from the main assembly

            // ✅ Register AutoMapper profiles ONCE
            services.AddAutoMapper(assembly);
            // ============================================
            // Currency Services
            // ============================================
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<ICurrencyService, CurrencyService>();

            // ============================================
            // Company Services
            // ============================================
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyService, CompanyService>();

            // ============================================
            // Treasury Services
            // ============================================
            services.AddScoped<ITreasuryService, TreasuryService>();

            return services;
        }
    }
}