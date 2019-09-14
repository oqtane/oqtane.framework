using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageService
    {
        Task<List<Page>> GetPagesAsync(int SiteId);
        Task<Page> GetPageAsync(int PageId);
        Task<Page> AddPageAsync(Page Page);
        Task<Page> UpdatePageAsync(Page Page);
        Task UpdatePageOrderAsync(int SiteId, int? ParentId);
        Task DeletePageAsync(int PageId);
    }
}
