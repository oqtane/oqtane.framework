using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {        
        private readonly SiteState _siteState;

        public HtmlTextService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string ApiUrl => CreateApiUrl(_siteState.Alias, "HtmlText");

        public async Task<HtmlTextInfo> GetHtmlTextAsync(int moduleId)
        {
            var htmltext = await GetJsonAsync<List<HtmlTextInfo>>($"{ApiUrl}/{moduleId}?entityid={moduleId}");
            return htmltext.FirstOrDefault();
        }

        public async Task AddHtmlTextAsync(HtmlTextInfo htmlText)
        {
            await PostJsonAsync($"{ApiUrl}?entityid={htmlText.ModuleId}", htmlText);
        }

        public async Task UpdateHtmlTextAsync(HtmlTextInfo htmlText)
        {
            await PutJsonAsync($"{ApiUrl}/{htmlText.HtmlTextId}?entityid={htmlText.ModuleId}", htmlText);
        }

        public async Task DeleteHtmlTextAsync(int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{moduleId}?entityid={moduleId}");
        }
    }
}
