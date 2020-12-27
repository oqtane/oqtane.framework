using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Infrastructure
{
    public abstract class ServerStartup : IServerOrderedStartup
    {
        public virtual int Order { get; } = 0;

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        public virtual void ConfigureMvc(IMvcBuilder mvcBuilder)
        {

        }

        public virtual void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
