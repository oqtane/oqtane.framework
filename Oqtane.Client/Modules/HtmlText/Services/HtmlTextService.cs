using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService, IService
    {        
        public HtmlTextService(HttpClient http, SiteState siteState) : base(http, siteState) {}

        private string ApiUrl => CreateApiUrl("HtmlText");

        public async Task<Models.HtmlText> GetHtmlTextAsync(int moduleId)
        {
            AddAuthorizationPolicyHeader(EntityNames.Module, moduleId);
            return await GetJsonAsync<Models.HtmlText>($"{ApiUrl}/{moduleId}");
        }

        public async Task AddHtmlTextAsync(Models.HtmlText htmlText)
        {
            AddAntiForgeryToken();
            AddAuthorizationPolicyHeader(EntityNames.Module, htmlText.ModuleId);
            await PostJsonAsync($"{ApiUrl}", htmlText);
        }

        public async Task UpdateHtmlTextAsync(Models.HtmlText htmlText)
        {
            AddAntiForgeryToken();
            AddAuthorizationPolicyHeader(EntityNames.Module, htmlText.ModuleId);
            await PutJsonAsync($"{ApiUrl}/{htmlText.HtmlTextId}", htmlText);
        }

        public async Task DeleteHtmlTextAsync(int moduleId)
        {
            AddAntiForgeryToken();
            AddAuthorizationPolicyHeader(EntityNames.Module, moduleId);
            await DeleteAsync($"{ApiUrl}/{moduleId}");
        }
    }
}
