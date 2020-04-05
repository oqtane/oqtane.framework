using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPageModuleService
    {
        Task<PageModule> GetPageModuleAsync(int pageModuleId);
        Task<PageModule> GetPageModuleAsync(int pageId, int moduleId);
        Task<PageModule> AddPageModuleAsync(PageModule pageModule);
        Task<PageModule> UpdatePageModuleAsync(PageModule pageModule);
        Task UpdatePageModuleOrderAsync(int pageId, string pane);
        Task DeletePageModuleAsync(int pageModuleId);
    }
}
