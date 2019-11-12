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
        private readonly NavigationManager NavigationManager;

        public TenantService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Tenant"); }
        }

        public async Task<List<Tenant>> GetTenantsAsync()
        {
            List<Tenant> tenants = await http.GetJsonAsync<List<Tenant>>(apiurl);
            return tenants.OrderBy(item => item.Name).ToList();
        }

        public async Task<Tenant> GetTenantAsync(int TenantId)
        {
            return await http.GetJsonAsync<Tenant>(apiurl + "/" + TenantId.ToString());
        }

        public async Task<Tenant> AddTenantAsync(Tenant Tenant)
        {
            return await http.PostJsonAsync<Tenant>(apiurl, Tenant);
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant Tenant)
        {
            return await http.PutJsonAsync<Tenant>(apiurl + "/" + Tenant.TenantId.ToString(), Tenant);
        }

        public async Task DeleteTenantAsync(int TenantId)
        {
            await http.DeleteAsync(apiurl + "/" + TenantId.ToString());
        }
    }
}
