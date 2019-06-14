using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<List<Tenant>> GetTenantsAsync()
        {
            List<Tenant> tenants = await http.GetJsonAsync<List<Tenant>>(apiurl);
            return tenants.OrderBy(item => item.Name).ToList();
        }

        public async Task<Tenant> GetTenantAsync()
        {
            return await http.GetJsonAsync<Tenant>(apiurl);
        }
    }
}
