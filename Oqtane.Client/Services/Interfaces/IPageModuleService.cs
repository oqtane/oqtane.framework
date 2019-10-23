using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageModuleService
    {
        Task<List<PageModule>> GetPageModulesAsync();
        Task<PageModule> GetPageModuleAsync(int PageModuleId);
        Task<PageModule> GetPageModuleAsync(int PageId, int ModuleId);
        Task<PageModule> AddPageModuleAsync(PageModule PageModule);
        Task<PageModule> UpdatePageModuleAsync(PageModule PageModule);
        Task UpdatePageModuleOrderAsync(int PageId, string Pane);
        Task DeletePageModuleAsync(int PageModuleId);
    }
}
