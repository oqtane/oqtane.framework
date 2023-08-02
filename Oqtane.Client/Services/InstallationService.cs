using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Net;
using System.Linq;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;
        private readonly HttpClient _http;

        public InstallationService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http, siteState)
        {
            _navigationManager = navigationManager;
            _siteState = siteState;
            _http = http;
        }

        private string ApiUrl => (_siteState.Alias == null)
            ? CreateApiUrl("Installation", null, ControllerRoutes.ApiRoute) // tenant agnostic needed for initial installation
            : CreateApiUrl("Installation", _siteState.Alias); 

        public async Task<Installation> IsInstalled()
        {
            var path = "";
            if (_http.DefaultRequestHeaders.UserAgent.ToString().Contains(Constants.MauiUserAgent))
            {
                path = _http.DefaultRequestHeaders.GetValues(Constants.MauiAliasPath).First();
            }
            else
            {
                path = new Uri(_navigationManager.Uri).LocalPath.Substring(1);
            }
            return await GetJsonAsync<Installation>($"{ApiUrl}/installed/?path={WebUtility.UrlEncode(path)}");
        }

        public async Task<Installation> Install(InstallConfig config)
        {
            return await PostJsonAsync<InstallConfig,Installation>(ApiUrl, config);
        }

        public async Task<Installation> Upgrade()
        {
            return await GetJsonAsync<Installation>($"{ApiUrl}/upgrade");
        }

        public async Task RestartAsync()
        {
            await PostAsync($"{ApiUrl}/restart");
        }

        public async Task RegisterAsync(string email)
        {
            await PostJsonAsync($"{ApiUrl}/register?email={WebUtility.UrlEncode(email)}", true);
        }
    }
}
