using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class PageService : ServiceBase, IPageService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public PageService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Page");
        }

        public async Task<List<Page>> GetPagesAsync(int SiteId)
        {
            List<Page> pages = await http.GetJsonAsync<List<Page>>(apiurl + "?siteid=" + SiteId.ToString());
            return pages.OrderBy(item => item.Order).ToList();
        }

        public async Task AddPageAsync(Page page)
        {
            await http.PostJsonAsync(apiurl, page);
        }

        public async Task UpdatePageAsync(Page page)
        {
            await http.PutJsonAsync(apiurl + "/" + page.PageId.ToString(), page);
        }
        public async Task DeletePageAsync(int PageId)
        {
            await http.DeleteAsync(apiurl + "/" + PageId.ToString());
        }
    }
}
