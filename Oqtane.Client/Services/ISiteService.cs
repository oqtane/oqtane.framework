using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetSitesAsync();

        Task<Site> GetSiteAsync(int SiteId);

        Task AddSiteAsync(Site site);

        Task UpdateSiteAsync(Site site);

        Task DeleteSiteAsync(int SiteId);
    }
}
