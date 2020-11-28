using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Infrastructure.Startup
{
    public interface IOqtaneServices
    {
        void AddAuthentication(IServiceCollection services);

        void AddLocalization(IServiceCollection services);

        void AddIdentity(IServiceCollection services);

        void AddDatabase(IServiceCollection services, string connectionString);
    }
}
