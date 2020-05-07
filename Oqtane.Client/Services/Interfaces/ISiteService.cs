using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        void SetAlias(Alias alias);

        Task<List<Site>> GetSitesAsync();

        Task<Site> GetSiteAsync(int siteId);

        Task<Site> AddSiteAsync(Site site);

        Task<Site> UpdateSiteAsync(Site site);

        Task DeleteSiteAsync(int siteId);
    }
}
