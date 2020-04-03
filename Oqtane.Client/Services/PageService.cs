using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using System.Net;

namespace Oqtane.Services
{
    public class PageService : ServiceBase, IPageService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public PageService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Page"); }
        }

        public async Task<List<Page>> GetPagesAsync(int siteId)
        {
            List<Page> pages = await _http.GetJsonAsync<List<Page>>($"{Apiurl}?siteid={siteId.ToString()}");
            pages = GetPagesHierarchy(pages);
            return pages;
        }

        public async Task<Page> GetPageAsync(int pageId)
        {
            return await _http.GetJsonAsync<Page>($"{Apiurl}/{pageId.ToString()}");
        }

        public async Task<Page> GetPageAsync(int pageId, int userId)
        {
            return await _http.GetJsonAsync<Page>($"{Apiurl}/{pageId.ToString()}?userid={userId.ToString()}");
        }

        public async Task<Page> GetPageAsync(string path, int siteId)
        {
            try
            {
                return await _http.GetJsonAsync<Page>($"{Apiurl}/path/{siteId.ToString()}?path={WebUtility.UrlEncode(path)}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Page> AddPageAsync(Page page)
        {
            return await _http.PostJsonAsync<Page>(Apiurl, page);
        }

        public async Task<Page> AddPageAsync(int pageId, int userId)
        {
            return await _http.PostJsonAsync<Page>($"{Apiurl}/{pageId.ToString()}?userid={userId.ToString()}", null);
        }

        public async Task<Page> UpdatePageAsync(Page page)
        {
            return await _http.PutJsonAsync<Page>($"{Apiurl}/{page.PageId.ToString()}", page);
        }

        public async Task UpdatePageOrderAsync(int siteId, int pageId, int? parentId)
        {
            var parent = parentId == null
                ? string.Empty
                : parentId.ToString();
            await _http.PutJsonAsync($"{Apiurl}/?siteid={siteId.ToString()}&pageid={pageId.ToString()}&parentid={parent}", null);
        }

        public async Task DeletePageAsync(int pageId)
        {
            await _http.DeleteAsync($"{Apiurl}/{pageId.ToString()}");
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
