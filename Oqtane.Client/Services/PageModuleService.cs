using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        
        private readonly SiteState _siteState;

        public PageModuleService(HttpClient http, SiteState siteState) : base(http)
        {            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "PageModule");

        public async Task<PageModule> GetPageModuleAsync(int pageModuleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageModuleId}");
        }

        public async Task<PageModule> GetPageModuleAsync(int pageId, int moduleId)
        {
            return await GetJsonAsync<PageModule>($"{Apiurl}/{pageId}/{moduleId}");
        }

        public async Task<PageModule> AddPageModuleAsync(PageModule pageModule)
        {
            return await PostJsonAsync<PageModule>(Apiurl, pageModule);
        }

        public async Task<PageModule> UpdatePageModuleAsync(PageModule pageModule)
        {
            return await PutJsonAsync<PageModule>($"{Apiurl}/{pageModule.PageModuleId}", pageModule);
        }

        public async Task UpdatePageModuleOrderAsync(int pageId, string pane)
        {
            await PutAsync($"{Apiurl}/?pageid={pageId}&pane={pane}");
        }

        public async Task DeletePageModuleAsync(int pageModuleId)
        {
            await DeleteAsync($"{Apiurl}/{pageModuleId}");
        }
    }
}
