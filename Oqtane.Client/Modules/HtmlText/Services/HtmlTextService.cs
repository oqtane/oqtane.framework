using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Services;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Shared;
using Oqtane.Models;

namespace Oqtane.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public HtmlTextService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "HtmlText"); }
        }

        public async Task<HtmlTextInfo> GetHtmlTextAsync(int ModuleId)
        {
            HtmlTextInfo htmltext;
            try
            {
                // exception handling is required because GetJsonAsync() returns an error if no content exists for the ModuleId ( https://github.com/aspnet/AspNetCore/issues/14041 )
                htmltext = await _http.GetJsonAsync<HtmlTextInfo>(apiurl + "/" + ModuleId.ToString() + "?entityid=" + ModuleId.ToString());
            }
            catch
            {
                htmltext = null;
            }
            return htmltext;
        }

        public async Task AddHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await _http.PostJsonAsync(apiurl + "?entityid=" + htmltext.ModuleId.ToString(), htmltext);
        }

        public async Task UpdateHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await _http.PutJsonAsync(apiurl + "/" + htmltext.HtmlTextId.ToString() + "?entityid=" + htmltext.ModuleId.ToString(), htmltext);
        }

        public async Task DeleteHtmlTextAsync(int ModuleId)
        {
            await _http.DeleteAsync(apiurl + "/" + ModuleId.ToString() + "?entityid=" + ModuleId.ToString());
        }

    }
}
