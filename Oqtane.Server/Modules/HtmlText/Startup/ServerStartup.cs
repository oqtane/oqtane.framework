using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Modules.HtmlText.Services;

namespace Oqtane.Modules.HtmlText.Startup
{
    public class ServerStartup : IServerStartup
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
            services.AddTransient<IHtmlTextService, ServerHtmlTextService>();
            services.AddDbContextFactory<HtmlTextContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
