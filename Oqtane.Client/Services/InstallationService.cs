using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public InstallationService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Installation"); }
        }

        public async Task<GenericResponse> IsInstalled()
        {
            return await _http.GetJsonAsync<GenericResponse>(apiurl + "/installed");
        }

        public async Task<GenericResponse> Install(string connectionstring)
        {
            return await _http.PostJsonAsync<GenericResponse>(apiurl, connectionstring);
        }

        public async Task<GenericResponse> Upgrade()
        {
            return await _http.GetJsonAsync<GenericResponse>(apiurl + "/upgrade");
        }
    }
}
