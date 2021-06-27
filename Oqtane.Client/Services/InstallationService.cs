using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Net;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;

        public InstallationService(HttpClient http, NavigationManager navigationManager, SiteState siteState) : base(http)
        {
            _navigationManager = navigationManager;
            _siteState = siteState;
        }

        private string ApiUrl => CreateApiUrl("Installation", null, ControllerRoutes.ApiRoute); // tenant agnostic

        public async Task<Installation> IsInstalled()
        {
            // add antiforgerytoken header so that it is included on all HttpClient calls for the lifetime of the app
            AddRequestHeader(Constants.AntiForgeryTokenHeaderName, _siteState.AntiForgeryToken);
            var path = new Uri(_navigationManager.Uri).LocalPath.Substring(1);            
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
