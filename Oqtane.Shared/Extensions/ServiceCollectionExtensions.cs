using Oqtane.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOqtaneDataProtection(this IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddSingleton<DataProtector>();

            return services;
        }
    }
}
