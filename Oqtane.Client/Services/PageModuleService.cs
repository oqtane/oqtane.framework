using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class PageModuleService : ServiceBase, IPageModuleService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly IUriHelper urihelper;

        public PageModuleService(HttpClient http, SiteState sitestate, IUriHelper urihelper)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "PageModule"); }
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
