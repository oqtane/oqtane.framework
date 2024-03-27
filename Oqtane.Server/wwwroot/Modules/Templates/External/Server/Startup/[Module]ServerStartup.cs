using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using [Owner].Module.[Module].Repository;
using [Owner].Module.[Module].Services;

namespace [Owner].Module.[Module].Startup
{
    public class [Module]ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<I[Module]Service, Server[Module]Service>();
            services.AddDbContextFactory<[Module]Context>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
