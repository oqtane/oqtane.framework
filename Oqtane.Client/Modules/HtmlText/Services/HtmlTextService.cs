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

        public HtmlTextService(HttpClient http, SiteState sitestate)
        {
            this.http = http;
            this.sitestate = sitestate;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, "HtmlText"); }
        }

        public async Task<List<HtmlTextInfo>> GetHtmlTextAsync(int ModuleId)
        {
            List<HtmlTextInfo> htmltext = await http.GetJsonAsync<List<HtmlTextInfo>>(apiurl);
            htmltext = htmltext
                .Where(item => item.ModuleId == ModuleId)
                .ToList();
            return htmltext;
        }

        public async Task AddHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await http.PostJsonAsync(apiurl, htmltext);
        }

        public async Task UpdateHtmlTextAsync(HtmlTextInfo htmltext)
        {
            await http.PutJsonAsync(apiurl + "/" + htmltext.HtmlTextId.ToString(), htmltext);
        }

        public async Task DeleteHtmlTextAsync(int HtmlTextId)
        {
            await http.DeleteAsync(apiurl + "/" + HtmlTextId.ToString());
        }
    }
}
