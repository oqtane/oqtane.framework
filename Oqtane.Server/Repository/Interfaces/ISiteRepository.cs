using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteRepository
    {
        IEnumerable<Site> GetSites();
        Site AddSite(Site site);
        Site UpdateSite(Site site);
        Site GetSite(int siteId);
        void DeleteSite(int siteId);
        void CreatePages(Site site, List<PageTemplate> pageTemplates);
    }
}
