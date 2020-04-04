using Oqtane.Models;
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

        public PageModuleService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "PageModule"); }
        }

        public async Task<PageModule> GetPageModuleAsync(int pageModuleId)
        {
            return await _http.GetJsonAsync<PageModule>($"{Apiurl}/{pageModuleId.ToString()}");
        }

        public async Task<PageModule> GetPageModuleAsync(int pageId, int moduleId)
        {
            return await _http.GetJsonAsync<PageModule>($"{Apiurl}/{pageId.ToString()}/{moduleId.ToString()}");
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule pageModule)
        {
            return await _http.PostJsonAsync<PageModule>(Apiurl, pageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule pageModule)
        {
            return await _http.PutJsonAsync<PageModule>($"{Apiurl}/{pageModule.PageModuleId.ToString()}", pageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int pageId, string pane)
        {
            await _http.PutJsonAsync($"{Apiurl}/?pageid={pageId.ToString()}&pane={pane}", null);
        }

        public async Task DeletePageModuleAsync(int pageModuleId)
        {
            await _http.DeleteAsync($"{Apiurl}/{pageModuleId.ToString()}");
        }
    }
}
