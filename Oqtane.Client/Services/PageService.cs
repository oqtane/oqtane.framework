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

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Page"); }
        }

        public async Task<List<Page>> GetPagesAsync(int SiteId)
        {
            List<Page> pages = await _http.GetJsonAsync<List<Page>>(apiurl + "?siteid=" + SiteId.ToString());
            pages = GetPagesHierarchy(pages);
            return pages;
        }

        public async Task<Page> GetPageAsync(int PageId)
        {
            return await _http.GetJsonAsync<Page>(apiurl + "/" + PageId.ToString());
        }

        public async Task<Page> GetPageAsync(int PageId, int UserId)
        {
            return await _http.GetJsonAsync<Page>(apiurl + "/" + PageId.ToString() + "?userid=" + UserId.ToString());
        }

        public async Task<Page> GetPageAsync(string Path, int SiteId)
        {
            try
            {
                return await _http.GetJsonAsync<Page>(apiurl + "/path/" + SiteId.ToString() + "?path=" + WebUtility.UrlEncode(Path));
            }
            catch
            {
                return null;
            }
        }

        public async Task<Page> AddPageAsync(Page Page)
        {
            return await _http.PostJsonAsync<Page>(apiurl, Page);
        }

        public async Task<Page> AddPageAsync(int PageId, int UserId)
        {
            return await _http.PostJsonAsync<Page>(apiurl + "/" + PageId.ToString() + "?userid=" + UserId.ToString(), null);
        }

        public async Task<Page> UpdatePageAsync(Page Page)
        {
            return await _http.PutJsonAsync<Page>(apiurl + "/" + Page.PageId.ToString(), Page);
        }

        public async Task UpdatePageOrderAsync(int SiteId, int PageId, int? ParentId)
        {
            await _http.PutJsonAsync(apiurl + "/?siteid=" + SiteId.ToString() + "&pageid=" + PageId.ToString() + "&parentid=" + ((ParentId == null) ? "" : ParentId.ToString()), null);
        }

        public async Task DeletePageAsync(int PageId)
        {
            await _http.DeleteAsync(apiurl + "/" + PageId.ToString());
        }

        private static List<Page> GetPagesHierarchy(List<Page> Pages)
        {
            List<Page> hierarchy = new List<Page>();
            Action<List<Page>, Page> GetPath = null;
            GetPath = (List<Page> pages, Page page) =>
            {
                IEnumerable<Page> children;
                int level;
                if (page == null)
                {
                    level = -1;
                    children = Pages.Where(item => item.ParentId == null);
                }
                else
                {
                    level = page.Level;
                    children = Pages.Where(item => item.ParentId == page.PageId);
                }
                foreach (Page child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = Pages.Where(item => item.ParentId == child.PageId).Any();
                    hierarchy.Add(child);
                    GetPath(pages, child);
                }
            };
            Pages = Pages.OrderBy(item => item.Order).ToList();
            GetPath(Pages, null);

            // add any non-hierarchical items to the end of the list
            foreach (Page page in Pages)
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
