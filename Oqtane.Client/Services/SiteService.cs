using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class SiteService : ServiceBase, ISiteService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public SiteService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Site");
        }

        public async Task<List<Site>> GetSitesAsync()
        {
            List<Site> sites = await http.GetJsonAsync<List<Site>>(apiurl);
            return sites.OrderBy(item => item.Name).ToList();
        }

        public async Task<Site> GetSiteAsync(int SiteId)
        {
            List<Site> sites = await http.GetJsonAsync<List<Site>>(apiurl);
            Site site;
            if (sites.Count == 1)
            {
                site = sites.FirstOrDefault();
            }
            else
            {
                site = sites.Where(item => item.SiteId == SiteId).FirstOrDefault();
            }
            return site;
        }

        public async Task AddSiteAsync(Site site)
        {
            await http.PostJsonAsync(apiurl, site);
        }

        public async Task UpdateSiteAsync(Site site)
        {
            await http.PutJsonAsync(apiurl + "/" + site.SiteId.ToString(), site);
        }
        public async Task DeleteSiteAsync(int SiteId)
        {
            await http.DeleteAsync(apiurl + "/" + SiteId.ToString());
        }
    }
}
