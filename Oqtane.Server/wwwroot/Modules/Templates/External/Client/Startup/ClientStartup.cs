using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using [Owner].Module.[Module].Services;

namespace [Owner].Module.[Module].Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<I[Module]Service, [Module]Service>();
        }
    }
}
