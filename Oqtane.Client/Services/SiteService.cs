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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public SiteService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Site"); }
        }

        public async Task<List<Site>> GetSitesAsync(Alias Alias)
        {
            List<Site> sites = await http.GetJsonAsync<List<Site>>(CreateCrossTenantUrl(apiurl, Alias));
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int SiteId, Alias Alias)
        {
            return await http.GetJsonAsync<Site>(CreateCrossTenantUrl(apiurl + "/" + SiteId.ToString(), Alias));
        }

        public async Task<Site> AddSiteAsync(Site Site, Alias Alias)
        {
            return await http.PostJsonAsync<Site>(CreateCrossTenantUrl(apiurl, Alias), Site);
        }

        public async Task<Site> UpdateSiteAsync(Site Site, Alias Alias)
        {
            return await http.PutJsonAsync<Site>(CreateCrossTenantUrl(apiurl + "/" + Site.SiteId.ToString(), Alias), Site);
        }

        public async Task DeleteSiteAsync(int SiteId, Alias Alias)
        {
            await http.DeleteAsync(CreateCrossTenantUrl(apiurl + "/" + SiteId.ToString(), Alias));
        }
    }
}
