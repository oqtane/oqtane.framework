using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetSitesAsync(Alias alias);

        Task<Site> GetSiteAsync(int siteId, Alias alias);

        Task<Site> AddSiteAsync(Site site, Alias alias);

        Task<Site> UpdateSiteAsync(Site site, Alias alias);

        Task DeleteSiteAsync(int siteId, Alias alias);
    }
}
