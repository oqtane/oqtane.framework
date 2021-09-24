using Oqtane.Documentation;
using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retreive <see cref="Site"/> entries
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

        [PrivateApi]
        [Obsolete("This method is deprecated.", false)]
        void SetAlias(Alias alias);
    }
}
