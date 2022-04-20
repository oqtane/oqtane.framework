using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteService : ServiceBase, ISiteService
    {
        public SiteService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Site");

        public async Task<List<Site>> GetSitesAsync()
        {
            List<Site> sites = await GetJsonAsync<List<Site>>(Apiurl);
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int siteId)
        {
            return await GetJsonAsync<Site>($"{Apiurl}/{siteId}");
        }

        public async Task<Site> AddSiteAsync(Site site)
        {
            return await PostJsonAsync<Site>(Apiurl, site);
        }

        public async Task<Site> UpdateSiteAsync(Site site)
        {
            return await PutJsonAsync<Site>($"{Apiurl}/{site.SiteId}", site);
        }

        public async Task DeleteSiteAsync(int siteId)
        {
            await DeleteAsync($"{Apiurl}/{siteId}");
        }

        [Obsolete("This method is deprecated.", false)]
        public void SetAlias(Alias alias)
        {
            base.Alias = alias;
        }
    }
}
