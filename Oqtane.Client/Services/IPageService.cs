using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageService
    {
        Task<List<Page>> GetPagesAsync(int SiteId);
        Task<Page> GetPageAsync(int PageId);
        Task AddPageAsync(Page page);
        Task UpdatePageAsync(Page page);
        Task DeletePageAsync(int PageId);
    }
}
