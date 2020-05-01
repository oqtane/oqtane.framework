using System;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;
        private readonly IServiceProvider _serviceProvider;

        public IdentityAuthenticationStateProvider(NavigationManager navigationManager, SiteState siteState, IServiceProvider serviceProvider)
        {
            _navigationManager = navigationManager;
            _siteState = siteState;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // get HttpClient lazily from IServiceProvider as you cannot use standard dependency injection due to the AuthenticationStateProvider being initialized prior to NavigationManager ( https://github.com/aspnet/AspNetCore/issues/11867 )
            var http = _serviceProvider.GetRequiredService<HttpClient>();
            string apiurl = ServiceBase.CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "User") + "/authenticate";
            User user = await http.GetFromJsonAsync<User>(apiurl);

            ClaimsIdentity identity = new ClaimsIdentity();
            if (user.IsAuthenticated)
            {
                identity = new ClaimsIdentity("Identity.Application");
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()));
                foreach (string role in user.Roles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
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
