using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetSitesAsync();

        Task<Site> GetSiteAsync(int siteId);

        Task<Site> AddSiteAsync(Site site);

        Task<Site> UpdateSiteAsync(Site site);

        Task DeleteSiteAsync(int siteId);

        [Obsolete("This method is deprecated.", false)]
        void SetAlias(Alias alias);
    }
}
