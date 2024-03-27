using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteRepository
    {
        // asynchronous methods
        Task<IEnumerable<Site>> GetSitesAsync();
        Task<Site> AddSiteAsync(Site site);
        Task<Site> UpdateSiteAsync(Site site);
        Task<Site> GetSiteAsync(int siteId);
        Task<Site> GetSiteAsync(int siteId, bool tracking);
        Task DeleteSiteAsync(int siteId);

        // synchronous methods
        IEnumerable<Site> GetSites();
        Site AddSite(Site site);
        Site UpdateSite(Site site);
        Site GetSite(int siteId);
        Site GetSite(int siteId, bool tracking);
        void DeleteSite(int siteId);

        void InitializeSite(Alias alias);
        void CreatePages(Site site, List<PageTemplate> pageTemplates, Alias alias);
    }
}
