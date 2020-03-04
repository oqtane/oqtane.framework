using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SiteService : ServiceBase, ISiteService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SiteService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Site"); }
        }

        public async Task<List<Site>> GetSitesAsync(Alias Alias)
        {
            List<Site> sites = await _http.GetJsonAsync<List<Site>>(CreateCrossTenantUrl(apiurl, Alias));
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int SiteId, Alias Alias)
        {
            return await _http.GetJsonAsync<Site>(CreateCrossTenantUrl(apiurl + "/" + SiteId.ToString(), Alias));
        }

        public async Task<Site> AddSiteAsync(Site Site, Alias Alias)
        {
            return await _http.PostJsonAsync<Site>(CreateCrossTenantUrl(apiurl, Alias), Site);
        }

        public async Task<Site> UpdateSiteAsync(Site Site, Alias Alias)
        {
            return await _http.PutJsonAsync<Site>(CreateCrossTenantUrl(apiurl + "/" + Site.SiteId.ToString(), Alias), Site);
        }

        public async Task DeleteSiteAsync(int SiteId, Alias Alias)
        {
            await _http.DeleteAsync(CreateCrossTenantUrl(apiurl + "/" + SiteId.ToString(), Alias));
        }
    }
}
