using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Oqtane.Services
{
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public PageModuleService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "PageModule");
        }

        public async Task<List<PageModule>> GetPageModulesAsync()
        {
            return await http.GetJsonAsync<List<PageModule>>(apiurl);
        }

        public async Task AddPageModuleAsync(PageModule pagemodule)
        {
            await http.PostJsonAsync(apiurl, pagemodule);
        }

        public async Task UpdatePageModuleAsync(PageModule pagemodule)
        {
            await http.PutJsonAsync(apiurl + "/" + pagemodule.PageModuleId.ToString(), pagemodule);
        }

        public async Task DeletePageModuleAsync(int PageModuleId)
        {
            await http.DeleteAsync(apiurl + "/" + PageModuleId.ToString());
        }
    }
}
