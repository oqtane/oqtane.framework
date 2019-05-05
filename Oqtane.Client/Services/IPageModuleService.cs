using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageModuleService
    {
        Task<List<PageModule>> GetPageModulesAsync();
        Task AddPageModuleAsync(PageModule pagemodule);
        Task UpdatePageModuleAsync(PageModule pagemodule);
        Task DeletePageModuleAsync(int PageModuleId);
    }
}
