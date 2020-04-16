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
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SiteService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Site"); }
        }

        public async Task<List<Site>> GetSitesAsync(Alias alias)
        {
            List<Site> sites = await GetJsonAsync<List<Site>>(CreateCrossTenantUrl(Apiurl, alias));
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int siteId, Alias alias)
        {
            return await GetJsonAsync<Site>(CreateCrossTenantUrl($"{Apiurl}/{siteId.ToString()}", alias));
        }

        public async Task<Site> AddSiteAsync(Site site, Alias alias)
        {
            return await PostJsonAsync<Site>(CreateCrossTenantUrl(Apiurl, alias), site);
        }

        public async Task<Site> UpdateSiteAsync(Site site, Alias alias)
        {
            return await PutJsonAsync<Site>(CreateCrossTenantUrl($"{Apiurl}/{site.SiteId.ToString()}", alias), site);
        }

        public async Task DeleteSiteAsync(int siteId, Alias alias)
        {
            await DeleteAsync(CreateCrossTenantUrl($"{Apiurl}/{siteId.ToString()}", alias));
        }
    }
}
