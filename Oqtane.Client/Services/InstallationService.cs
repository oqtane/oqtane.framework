using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public InstallationService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Installation"); }
        }

        public async Task<Installation> IsInstalled()
        {
            return await _http.GetJsonAsync<Installation>(Apiurl + "/installed");
        }

        public async Task<Installation> Install(string connectionstring)
        {
            return await _http.PostJsonAsync<Installation>(Apiurl, connectionstring);
        }

        public async Task<Installation> Upgrade()
        {
            return await _http.GetJsonAsync<Installation>(Apiurl + "/upgrade");
        }
    }
}
