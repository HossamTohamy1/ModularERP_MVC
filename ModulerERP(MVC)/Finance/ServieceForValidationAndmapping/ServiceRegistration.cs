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

            return services;
        }
    }
}