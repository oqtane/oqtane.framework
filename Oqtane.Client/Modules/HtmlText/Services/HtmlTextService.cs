using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Services;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public HtmlTextService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsoluteUri, "HtmlText"); }
        }

        public async Task<HtmlTextInfo> GetHtmlTextAsync(int ModuleId)
        {
            return await http.GetJsonAsync<HtmlTextInfo>(apiurl + "/" + ModuleId.ToString() + "?entityid=" + ModuleId.ToString());
        }

        public async Task AddHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await http.PostJsonAsync(apiurl + "?entityid=" + htmltext.ModuleId.ToString(), htmltext);
        }

        public async Task UpdateHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await http.PutJsonAsync(apiurl + "/" + htmltext.HtmlTextId.ToString() + "?entityid=" + htmltext.ModuleId.ToString(), htmltext);
        }

        public async Task DeleteHtmlTextAsync(int ModuleId)
        {
            await http.DeleteAsync(apiurl + "/" + ModuleId.ToString() + "?entityid=" + ModuleId.ToString());
        }
    }
}
