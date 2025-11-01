using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using [Owner].Module.[Module].Services;

namespace [Owner].Module.[Module].Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(I[Module]Service)))
            {
                services.AddScoped<I[Module]Service, [Module]Service>();
            }
        }
    }
}
