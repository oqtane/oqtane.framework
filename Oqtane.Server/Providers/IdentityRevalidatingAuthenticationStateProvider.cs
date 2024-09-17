using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System;
using Oqtane.Infrastructure;
using Oqtane.Extensions;
using Oqtane.Managers;

namespace Oqtane.Providers
{
    public class IdentityRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, IOptions<IdentityOptions> options) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
    {
        // defines how often the authentication state should be asynchronously validated
        // note that there is no property within IdentityOptions which allows this to be customized 
        protected override TimeSpan RevalidationInterval
        {
            get
            {
                // suppresses the unused compiler warning for options
                var revalidationInterval = TimeSpan.FromMinutes(30); // default Identity value
                return (options != null) ? revalidationInterval : revalidationInterval;
            }
        }

        protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authState, CancellationToken cancellationToken)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
            tenantManager.SetTenant(authState.User.TenantId());
            var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
            var user = userManager.GetUser(authState.User.Identity.Name, authState.User.SiteId());
            if (user == null || user.IsDeleted)
            {
                return false;
            }
            else
            {
                return authState.User.SecurityStamp() == user.SecurityStamp;
            }
        }
    }
}
