using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly NavigationManager NavigationManager;
        private readonly SiteState sitestate;
        private readonly IServiceProvider provider;

        public IdentityAuthenticationStateProvider(NavigationManager NavigationManager, SiteState sitestate, IServiceProvider provider)
        {
            this.NavigationManager = NavigationManager;
            this.sitestate = sitestate;
            this.provider = provider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // get HttpClient lazily from IServiceProvider as you cannot use standard dependency injection due to the AuthenticationStateProvider being initialized prior to NavigationManager ( https://github.com/aspnet/AspNetCore/issues/11867 )
            var http = provider.GetRequiredService<HttpClient>();
            string apiurl = ServiceBase.CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "User") + "/authenticate";
            User user = await http.GetJsonAsync<User>(apiurl);

            ClaimsIdentity identity = new ClaimsIdentity();
            if (user.IsAuthenticated)
            {
                identity = new ClaimsIdentity("Identity.Application");
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()));
                foreach (string role in user.Roles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
