using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;

namespace Oqtane.Providers
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        //private readonly IUserService UserService;
        private readonly IUriHelper urihelper;

        public ServerAuthenticationStateProvider(IUriHelper urihelper)
        {
            //this.UserService = UserService;
            this.urihelper = urihelper;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // hack: create a new HttpClient rather than relying on the registered service as the AuthenticationStateProvider is initialized prior to IUriHelper ( https://github.com/aspnet/AspNetCore/issues/11867 )
            HttpClient http = new HttpClient();
            Uri uri = new Uri(urihelper.GetAbsoluteUri());
            string apiurl = uri.Scheme + "://" + uri.Authority + "/~/api/User/authenticate";
            User user = await http.GetJsonAsync<User>(apiurl);
            var identity = user.IsAuthenticated
                ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }, "Identity.Application")
                : new ClaimsIdentity();
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
