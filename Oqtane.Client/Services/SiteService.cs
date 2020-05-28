using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SiteService : ServiceBase, ISiteService
    {
        
        private readonly SiteState _siteState;

        public SiteService(HttpClient http, SiteState siteState) : base(http)
        {
            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Site");

        public void SetAlias(Alias alias)
        {
            base.Alias = alias;
        }

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
    }
}
