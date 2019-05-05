using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Services;
using Oqtane.Shared.Modules.HtmlText.Models;

namespace Oqtane.Client.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public HtmlTextService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "HtmlText");
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
