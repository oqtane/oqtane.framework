using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public PageModuleService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "PageModule"); }
        }

        public async Task<PageModule> GetPageModuleAsync(int pageModuleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageModuleId.ToString()}");
        }

        public async Task<PageModule> GetPageModuleAsync(int pageId, int moduleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageId.ToString()}/{moduleId.ToString()}");
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule pageModule)
        {
            return await PostJsonAsync<PageModule>(Apiurl, pageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule pageModule)
        {
            return await PutJsonAsync<PageModule>($"{Apiurl}/{pageModule.PageModuleId.ToString()}", pageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int pageId, string pane)
        {
            await PutAsync($"{Apiurl}/?pageid={pageId.ToString()}&pane={pane}");
        }

        public async Task DeletePageModuleAsync(int pageModuleId)
        {
            await DeleteAsync($"{Apiurl}/{pageModuleId.ToString()}");
        }
    }
}
