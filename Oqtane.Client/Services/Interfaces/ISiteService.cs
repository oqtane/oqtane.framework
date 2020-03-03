using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetSitesAsync(Alias Alias);

        Task<Site> GetSiteAsync(int SiteId, Alias Alias);

        Task<Site> AddSiteAsync(Site Site, Alias Alias);

        Task<Site> UpdateSiteAsync(Site Site, Alias Alias);

        Task DeleteSiteAsync(int SiteId, Alias Alias);
    }
}
