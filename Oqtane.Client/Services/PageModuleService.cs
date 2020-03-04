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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public PageModuleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "PageModule"); }
        }

        public async Task<PageModule> GetPageModuleAsync(int PageModuleId)
        {
            return await _http.GetJsonAsync<PageModule>(apiurl + "/" + PageModuleId.ToString());
        }

        public async Task<PageModule> GetPageModuleAsync(int PageId, int ModuleId)
        {
            return await _http.GetJsonAsync<PageModule>(apiurl + "/" + PageId.ToString() + "/" + ModuleId.ToString());
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule PageModule)
        {
            return await _http.PostJsonAsync<PageModule>(apiurl, PageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule PageModule)
        {
            return await _http.PutJsonAsync<PageModule>(apiurl + "/" + PageModule.PageModuleId.ToString(), PageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int PageId, string Pane)
        {
            await _http.PutJsonAsync(apiurl + "/?pageid=" + PageId.ToString() + "&pane=" + Pane, null);
        }

        public async Task DeletePageModuleAsync(int PageModuleId)
        {
            await _http.DeleteAsync(apiurl + "/" + PageModuleId.ToString());
        }
    }
}
