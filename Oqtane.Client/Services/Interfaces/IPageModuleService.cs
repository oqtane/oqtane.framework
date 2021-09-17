using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retreive a <see cref="PageModule"/>
    /// </summary>
    public interface IPageModuleService
    {

        /// <summary>
        /// Returns a specific page module
        /// </summary>
        /// <param name="pageModuleId"></param>
        /// <returns></returns>
        Task<PageModule> GetPageModuleAsync(int pageModuleId);

        /// <summary>
        /// Return a specific page module
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        Task<PageModule> GetPageModuleAsync(int pageId, int moduleId);

        /// <summary>
        /// Creates a new page module
        /// </summary>
        /// <param name="pageModule"></param>
        /// <returns></returns>
        Task<PageModule> AddPageModuleAsync(PageModule pageModule);

        /// <summary>
        /// Updates a existing page module
        /// </summary>
        /// <param name="pageModule"></param>
        /// <returns></returns>
        Task<PageModule> UpdatePageModuleAsync(PageModule pageModule);

        /// <summary>
        /// Updates order of all page modules in the given pane
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="pane"></param>
        /// <returns></returns>
        Task UpdatePageModuleOrderAsync(int pageId, string pane);

        /// <summary>
        /// Deletes a page module
        /// </summary>
        /// <param name="pageModuleId"></param>
        /// <returns></returns>
        Task DeletePageModuleAsync(int pageModuleId);
    }
}
