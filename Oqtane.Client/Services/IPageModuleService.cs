using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageModuleService
    {
        Task<List<PageModule>> GetPageModulesAsync();
        Task<PageModule> AddPageModuleAsync(PageModule PageModule);
        Task<PageModule> UpdatePageModuleAsync(PageModule PageModule);
        Task DeletePageModuleAsync(int PageModuleId);
    }
}
