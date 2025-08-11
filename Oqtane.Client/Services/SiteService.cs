using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retrieve <see cref="Site"/> entries
    /// </summary>
    public interface ISiteService
    {

        /// <summary>
        /// Returns a list of sites
        /// </summary>
        /// <returns></returns>
        Task<List<Site>> GetSitesAsync();

        /// <summary>
        /// Returns a specific site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<Site> GetSiteAsync(int siteId);

        /// <summary>
        /// Creates a new site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Site> AddSiteAsync(Site site);

        /// <summary>
        /// Updates an existing site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task<Site> UpdateSiteAsync(Site site);

        /// <summary>
        /// Deletes a site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task DeleteSiteAsync(int siteId);

        /// <summary>
        /// Returns a list of modules
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<List<Module>> GetModulesAsync(int siteId, int pageId);

        [PrivateApi]
        [Obsolete("This method is deprecated.", false)]
        void SetAlias(Alias alias);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteService : ServiceBase, ISiteService
    {
        public SiteService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Site");

        public async Task<List<Site>> GetSitesAsync()
        {
            return await GetJsonAsync<List<Site>>(Apiurl);
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

        public async Task<List<Module>> GetModulesAsync(int siteId, int pageId)
        {
            return await GetJsonAsync<List<Module>>($"{Apiurl}/modules/{siteId}/{pageId}");
        }

        [Obsolete("This method is deprecated.", false)]
        public void SetAlias(Alias alias)
        {
            base.Alias = alias;
        }
    }
}
