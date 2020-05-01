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
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public TenantService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Tenant"); }
        }

        public async Task<List<Tenant>> GetTenantsAsync()
        {
            List<Tenant> tenants = await GetJsonAsync<List<Tenant>>(Apiurl);
            return tenants.OrderBy(item => item.Name).ToList();
        }

        public async Task<Tenant> GetTenantAsync(int tenantId)
        {
            return await GetJsonAsync<Tenant>($"{Apiurl}/{tenantId.ToString()}");
        }

        public async Task<Tenant> AddTenantAsync(Tenant tenant)
        {
            return await PostJsonAsync<Tenant>(Apiurl, tenant);
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
        {
            return await PutJsonAsync<Tenant>($"{Apiurl}/{tenant.TenantId.ToString()}", tenant);
        }

        public async Task DeleteTenantAsync(int tenantId)
        {
            await DeleteAsync($"{Apiurl}/{tenantId.ToString()}");
        }
    }
}
