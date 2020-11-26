using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class IdentityStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
        }
    }
}
