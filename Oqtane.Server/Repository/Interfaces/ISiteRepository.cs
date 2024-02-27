using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteRepository
    {
        IEnumerable<Site> GetSites();
        Task<IEnumerable<Site>> GetSitesAsync();
        Site AddSite(Site site);
        Task<Site> AddSiteAsync(Site site);
        Site UpdateSite(Site site);
        Task<Site> UpdateSiteAsync(Site site);
        Site GetSite(int siteId);
        Task<Site> GetSiteAsync(int siteId);
        Site GetSite(int siteId, bool tracking);
        Task<Site> GetSiteAsync(int siteId, bool tracking);
        void DeleteSite(int siteId);
        Task DeleteSiteAsync(int siteId);
        void InitializeSite(Alias alias);
        void CreatePages(Site site, List<PageTemplate> pageTemplates, Alias alias);
    }
}
