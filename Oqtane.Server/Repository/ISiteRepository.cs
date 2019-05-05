using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteRepository
    {
        IEnumerable<Site> GetSites();
        void AddSite(Site site);
        void UpdateSite(Site site);
        Site GetSite(int siteId);
        void DeleteSite(int siteId);
    }
}
