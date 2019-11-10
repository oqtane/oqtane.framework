using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;
using System;

namespace Oqtane.Services
{
    public class PageService : ServiceBase, IPageService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public PageService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Page"); }
        }

        public async Task<List<Page>> GetPagesAsync(int SiteId)
        {
            List<Page> pages = await http.GetJsonAsync<List<Page>>(apiurl + "?siteid=" + SiteId.ToString());
            pages = GetPagesHierarchy(pages);
            return pages;
        }

        public async Task<Page> GetPageAsync(int PageId)
        {
            return await http.GetJsonAsync<Page>(apiurl + "/" + PageId.ToString());
        }

        public async Task<Page> GetPageAsync(int PageId, int UserId)
        {
            return await http.GetJsonAsync<Page>(apiurl + "/" + PageId.ToString() + "?userid=" + UserId.ToString());
        }

        public async Task<Page> AddPageAsync(Page Page)
        {
            return await http.PostJsonAsync<Page>(apiurl, Page);
        }

        public async Task<Page> UpdatePageAsync(Page Page)
        {
            return await http.PutJsonAsync<Page>(apiurl + "/" + Page.PageId.ToString(), Page);
        }

        public async Task UpdatePageOrderAsync(int SiteId, int PageId, int? ParentId)
        {
            await http.PutJsonAsync(apiurl + "/?siteid=" + SiteId.ToString() + "&pageid=" + PageId.ToString() + "&parentid=" + ((ParentId == null) ? "" : ParentId.ToString()), null);
        }

        public async Task DeletePageAsync(int PageId)
        {
            await http.DeleteAsync(apiurl + "/" + PageId.ToString());
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
            return hierarchy;
        }
    }
}
