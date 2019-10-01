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

        public async Task<List<Site>> GetSitesAsync()
        {
            List<Site> sites = await http.GetJsonAsync<List<Site>>(apiurl);
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int SiteId)
        {
            return await http.GetJsonAsync<Site>(apiurl + "/" + SiteId.ToString());
        }

        public async Task<Site> AddSiteAsync(Site Site)
        {
            return await http.PostJsonAsync<Site>(apiurl, Site);
        }

        public async Task<Site> UpdateSiteAsync(Site Site)
        {
            return await http.PutJsonAsync<Site>(apiurl + "/" + Site.SiteId.ToString(), Site);
        }

        public async Task DeleteSiteAsync(int SiteId)
        {
            await http.DeleteAsync(apiurl + "/" + SiteId.ToString());
        }
    }
}
