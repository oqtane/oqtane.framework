using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Services;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Shared;

namespace Oqtane.Client.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly IUriHelper urihelper;

        public HtmlTextService(HttpClient http, SiteState sitestate, IUriHelper urihelper)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "HtmlText"); }
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
