using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationManager _navigationManager;
 
        public IdentityAuthenticationStateProvider(IServiceProvider serviceProvider, NavigationManager navigationManager)
        {
            _serviceProvider = serviceProvider;
            _navigationManager = navigationManager;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity = new ClaimsIdentity();

            // get HttpClient lazily from IServiceProvider as you cannot use standard dependency injection due to the AuthenticationStateProvider being initialized prior to NavigationManager(https://github.com/aspnet/AspNetCore/issues/11867 )
            var http = _serviceProvider.GetRequiredService<HttpClient>();
            // get alias as SiteState has not been initialized ( cannot use AliasService as it is not yet registered )
            var path = new Uri(_navigationManager.Uri).LocalPath.Substring(1);
            var alias = await http.GetFromJsonAsync<Alias>($"/api/Alias/name/?path={WebUtility.UrlEncode(path)}&sync={DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}");
            // get user
            User user = await http.GetFromJsonAsync<User>(Utilities.TenantUrl(alias, "/api/User/authenticate"));
            if (user.IsAuthenticated)
            {
                identity = UserSecurity.CreateClaimsIdentity(alias, user);
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
