using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public PageModuleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "PageModule"); }
        }

        public async Task<List<PageModule>> GetPageModulesAsync()
        {
            return await http.GetJsonAsync<List<PageModule>>(apiurl);
        }

        public async Task<PageModule> GetPageModuleAsync(int PageModuleId)
        {
            return await http.GetJsonAsync<PageModule>(apiurl + "/" + PageModuleId.ToString());
        }

        public async Task<PageModule> GetPageModuleAsync(int PageId, int ModuleId)
        {
            return await http.GetJsonAsync<PageModule>(apiurl + "/" + PageId.ToString() + "/" + ModuleId.ToString());
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule PageModule)
        {
            return await http.PostJsonAsync<PageModule>(apiurl, PageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule PageModule)
        {
            return await http.PutJsonAsync<PageModule>(apiurl + "/" + PageModule.PageModuleId.ToString(), PageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int PageId, string Pane)
        {
            await http.PutJsonAsync(apiurl + "/?pageid=" + PageId.ToString() + "&pane=" + Pane, null);
        }

        public async Task DeletePageModuleAsync(int PageModuleId)
        {
            await http.DeleteAsync(apiurl + "/" + PageModuleId.ToString());
        }
    }
}
