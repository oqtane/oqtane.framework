using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Repository;

namespace Oqtane.Infrastructure.Startup
{
    public class DefaultOqtaneServerServices : IOqtaneServices
    {
        public virtual void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme);
        }

        public virtual void AddDatabase(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<MasterDBContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<TenantDBContext>();
        }

        public virtual void AddIdentity(IServiceCollection services)
        {
            services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
        }

        public virtual void AddLocalization(IServiceCollection services)
        {
            services.AddLocalization();
        }
    }
}
