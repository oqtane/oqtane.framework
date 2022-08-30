using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System.Net;
using Oqtane.Documentation;

namespace Oqtane.Services
{
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

        public async Task<Page> GetPageAsync(int pageId, int userId)
        {
            return await GetJsonAsync<Page>($"{Apiurl}/{pageId}?userid={userId}");
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
