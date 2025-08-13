using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retrieve a <see cref="PageModule"/>
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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        public PageModuleService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("PageModule");

        public async Task<PageModule> GetPageModuleAsync(int pageModuleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageModuleId}");
        }

        public async Task<PageModule> GetPageModuleAsync(int pageId, int moduleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageId}/{moduleId}");
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule pageModule)
        {
            return await PostJsonAsync<PageModule>(Apiurl, pageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule pageModule)
        {
            return await PutJsonAsync<PageModule>($"{Apiurl}/{pageModule.PageModuleId}", pageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int pageId, string pane)
        {
            await PutAsync($"{Apiurl}/?pageid={pageId}&pane={pane}");
        }

        public async Task DeletePageModuleAsync(int pageModuleId)
        {
            await DeleteAsync($"{Apiurl}/{pageModuleId}");
        }
    }
}
