using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using System.Net;

namespace Oqtane.Services
{
    public class PageService : ServiceBase, IPageService
    {
        
        private readonly SiteState _siteState;

        public PageService(HttpClient http, SiteState siteState) : base(http)
        {
            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Page");

        public async Task<List<Page>> GetPagesAsync(int siteId)
        {
            List<Page> pages = await GetJsonAsync<List<Page>>($"{Apiurl}?siteid={siteId}");
            pages = GetPagesHierarchy(pages);
            return pages;
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

        private static List<Page> GetPagesHierarchy(List<Page> pages)
        {
            List<Page> hierarchy = new List<Page>();
            Action<List<Page>, Page> getPath = null;
            getPath = (pageList, page) =>
            {
                IEnumerable<Page> children;
                int level;
                if (page == null)
                {
                    level = -1;
                    children = pages.Where(item => item.ParentId == null);
                }
                else
                {
                    level = page.Level;
                    children = pages.Where(item => item.ParentId == page.PageId);
                }
                foreach (Page child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = pages.Any(item => item.ParentId == child.PageId);
                    hierarchy.Add(child);
                    getPath(pageList, child);
                }
            };
            pages = pages.OrderBy(item => item.Order).ToList();
            getPath(pages, null);

            // add any non-hierarchical items to the end of the list
            foreach (Page page in pages)
            {
                if (hierarchy.Find(item => item.PageId == page.PageId) == null)
                {
                    hierarchy.Add(page);
                }
            }
            return hierarchy;
        }
    }
}
