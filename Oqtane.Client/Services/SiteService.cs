using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;
using System.Net;

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

        private string urlsuffix(Alias Alias)
        {
            string querystring = "";
            if (Alias != null)
            {
                querystring = "?alias=" + WebUtility.UrlEncode(Alias.Name);
            }
            return querystring;
        }

        public async Task<List<Site>> GetSitesAsync(Alias Alias)
        {
            List<Site> sites = await http.GetJsonAsync<List<Site>>(apiurl + urlsuffix(Alias));
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int SiteId, Alias Alias)
        {
            return await http.GetJsonAsync<Site>(apiurl + "/" + SiteId.ToString() + urlsuffix(Alias));
        }

        public async Task<Site> AddSiteAsync(Site Site, Alias Alias)
        {
            return await http.PostJsonAsync<Site>(apiurl + urlsuffix(Alias), Site);
        }

        public async Task<Site> UpdateSiteAsync(Site Site, Alias Alias)
        {
            return await http.PutJsonAsync<Site>(apiurl + "/" + Site.SiteId.ToString() + urlsuffix(Alias), Site);
        }

        public async Task DeleteSiteAsync(int SiteId, Alias Alias)
        {
            await http.DeleteAsync(apiurl + "/" + SiteId.ToString() + urlsuffix(Alias));
        }
    }
}
