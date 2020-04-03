using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Services.Interfaces;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SiteService : ServiceBase, ISiteService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SiteService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Site"); }
        }

        public async Task<List<Site>> GetSitesAsync(Alias alias)
        {
            List<Site> sites = await _http.GetJsonAsync<List<Site>>(CreateCrossTenantUrl(Apiurl, alias));
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int siteId, Alias alias)
        {
            return await _http.GetJsonAsync<Site>(CreateCrossTenantUrl($"{Apiurl}/{siteId.ToString()}", alias));
        }

        public async Task<Site> AddSiteAsync(Site site, Alias alias)
        {
            return await _http.PostJsonAsync<Site>(CreateCrossTenantUrl(Apiurl, alias), site);
        }

        public async Task<Site> UpdateSiteAsync(Site site, Alias alias)
        {
            return await _http.PutJsonAsync<Site>(CreateCrossTenantUrl($"{Apiurl}/{site.SiteId.ToString()}", alias), site);
        }

        public async Task DeleteSiteAsync(int siteId, Alias alias)
        {
            await _http.DeleteAsync(CreateCrossTenantUrl($"{Apiurl}/{siteId.ToString()}", alias));
        }
    }
}
