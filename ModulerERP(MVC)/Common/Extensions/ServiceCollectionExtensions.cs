using ModulerERP_MVC_.Common.Middleware;
using ModulerERP_MVC_.Common.Repositories.Interfaces;

namespace ModulerERP_MVC_.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<GlobalErrorHandlerMiddleware>();
            services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

            return services;
        }
    }
}