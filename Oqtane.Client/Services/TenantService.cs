using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Services
{
    public class TenantService : ServiceBase, ITenantService
    {
        public TenantService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("Tenant");

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
