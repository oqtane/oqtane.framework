using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IPageService
    {
        Task<List<Page>> GetPagesAsync(int siteId);
        Task<Page> GetPageAsync(int pageId);
        Task<Page> GetPageAsync(int pageId, int userId);
        Task<Page> GetPageAsync(string path, int siteId);
        Task<Page> AddPageAsync(Page page);
        Task<Page> AddPageAsync(int pageId, int userId);
        Task<Page> UpdatePageAsync(Page page);
        Task UpdatePageOrderAsync(int siteId, int pageId, int? parentId);
        Task DeletePageAsync(int pageId);
    }
}
