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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public InstallationService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Installation"); }
        }

        public async Task<GenericResponse> IsInstalled()
        {
            return await http.GetJsonAsync<GenericResponse>(apiurl + "/installed");
        }

        public async Task<GenericResponse> Install(string connectionstring)
        {
            return await http.PostJsonAsync<GenericResponse>(apiurl, connectionstring);
        }

        public async Task<GenericResponse> Upgrade()
        {
            return await http.GetJsonAsync<GenericResponse>(apiurl + "/upgrade");
        }
    }
}
