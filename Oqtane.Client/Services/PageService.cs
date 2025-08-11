using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System.Net;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    /// <summary>
    /// Services to store and retrieve a <see cref="Page"/>
    /// </summary>
    public interface IPageService
    {
        /// <summary>
        /// Returns a list of pages
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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class PageService : ServiceBase, IPageService
    {
        public PageService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Page");

        public async Task<List<Page>> GetPagesAsync(int siteId)
        {
            return await GetJsonAsync<List<Page>>($"{Apiurl}?siteid={siteId}");
        }

        public async Task<Page> GetPageAsync(int pageId)
        {
            return await GetJsonAsync<Page>($"{Apiurl}/{pageId}");
        }

        public async Task<Page> GetPageAsync(string path, int siteId)
        {
            try
            {
                return await GetJsonAsync<Page>($"{Apiurl}/path/{siteId}?path={WebUtility.UrlEncode(path)}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Page> AddPageAsync(Page page)
        {
            return await PostJsonAsync<Page>(Apiurl, page);
        }

        public async Task<Page> AddPageAsync(int pageId, int userId)
        {
            return await PostJsonAsync<Page>($"{Apiurl}/{pageId}?userid={userId}", null);
        }

        public async Task<Page> UpdatePageAsync(Page page)
        {
            return await PutJsonAsync<Page>($"{Apiurl}/{page.PageId}", page);
        }

        public async Task UpdatePageOrderAsync(int siteId, int pageId, int? parentId)
        {
            var parent = parentId == null
                ? string.Empty
                : parentId.ToString();
            await PutAsync($"{Apiurl}/?siteid={siteId}&pageid={pageId}&parentid={parent}");
        }

        public async Task DeletePageAsync(int pageId)
        {
            await DeleteAsync($"{Apiurl}/{pageId}");
        }
    }
}
