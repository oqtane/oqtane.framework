using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class TenantService : ServiceBase, ITenantService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;

        public TenantService(HttpClient http, SiteState sitestate)
        {
            this.http = http;
            this.sitestate = sitestate;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, "Tenant"); }
        }

        public async Task<Tenant> GetTenantAsync()
        {
            return await http.GetJsonAsync<Tenant>(apiurl);
        }
    }
}
