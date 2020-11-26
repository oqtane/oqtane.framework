using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Infrastructure
{
    public interface IServerStartup
    {
        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <remarks>The default services should (only) have negative values, so they can be executed first.</remarks>
        int Order { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        void ConfigureServices(IServiceCollection services);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        void Configure(IApplicationBuilder app, IWebHostEnvironment env);

        void ConfigureMvc(IMvcBuilder mvcBuilder);
    }
}

