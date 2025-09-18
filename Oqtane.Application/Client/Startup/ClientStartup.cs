using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using Oqtane.Application.Services;

namespace Oqtane.Application.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IMyModuleService)))
            {
                services.AddScoped<IMyModuleService, MyModuleService>();
            }
        }
    }
}
