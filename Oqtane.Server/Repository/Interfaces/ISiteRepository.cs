using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteRepository
    {
        IEnumerable<Site> GetSites();
        Site AddSite(Site Site);
        Site UpdateSite(Site Site);
        Site GetSite(int SiteId);
        void DeleteSite(int SiteId);
    }
}
