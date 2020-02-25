using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetSitesAsync();

        Task<Site> GetSiteAsync(int SiteId);

        Task<Site> AddSiteAsync(Site Site, Alias Alias);

        Task<Site> UpdateSiteAsync(Site Site);

        Task DeleteSiteAsync(int SiteId);
    }
}
