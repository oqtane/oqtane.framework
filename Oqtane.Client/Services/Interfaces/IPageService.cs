using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Services to store and retrieve a <see cref="Page"/>
    /// </summary>
    public interface IPageService
    {
        /// <summary>
        /// Retuns a list of pages
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Page>> GetPagesAsync(int siteId);

        /// <summary>
        /// Returns a specific page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<Page> GetPageAsync(int pageId);

        /// <summary>
        /// Returns a specific page personalized for the given user
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Page> GetPageAsync(int pageId, int userId);

        /// <summary>
        /// Returns a specific page by its defined path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<Page> GetPageAsync(string path, int siteId);

        /// <summary>
        /// Adds a new page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> AddPageAsync(Page page);

        /// <summary>
        /// Adds a new page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> AddPageAsync(int pageId, int userId);

        /// <summary>
        /// Updates a existing page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> UpdatePageAsync(Page page);

        /// <summary>
        /// Updates order of all page modules in the given parent
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="pageId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Task UpdatePageOrderAsync(int siteId, int pageId, int? parentId);

        /// <summary>
        /// Deletes a page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task DeletePageAsync(int pageId);
    }
}
