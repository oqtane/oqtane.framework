using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class TenantService : ServiceBase, ITenantService
    {
        private readonly SiteState _siteState;

        public TenantService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Tenant");

        public async Task<List<Tenant>> GetTenantsAsync()
        {
            List<Tenant> tenants = await GetJsonAsync<List<Tenant>>(Apiurl);
            return tenants.OrderBy(item => item.Name).ToList();
        }

        public async Task<Tenant> GetTenantAsync(int tenantId)
        {
            return await GetJsonAsync<Tenant>($"{Apiurl}/{tenantId}");
        }

        public async Task<Tenant> AddTenantAsync(Tenant tenant)
        {
            return await PostJsonAsync<Tenant>(Apiurl, tenant);
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
        {
            return await PutJsonAsync<Tenant>($"{Apiurl}/{tenant.TenantId}", tenant);
        }

        public async Task DeleteTenantAsync(int tenantId)
        {
            await DeleteAsync($"{Apiurl}/{tenantId}");
        }
    }
}
