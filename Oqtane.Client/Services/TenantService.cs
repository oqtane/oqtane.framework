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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public TenantService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Tenant"); }
        }

        public async Task<List<Tenant>> GetTenantsAsync()
        {
            List<Tenant> tenants = await _http.GetJsonAsync<List<Tenant>>(apiurl);
            return tenants.OrderBy(item => item.Name).ToList();
        }

        public async Task<Tenant> GetTenantAsync(int TenantId)
        {
            return await _http.GetJsonAsync<Tenant>(apiurl + "/" + TenantId.ToString());
        }

        public async Task<Tenant> AddTenantAsync(Tenant Tenant)
        {
            return await _http.PostJsonAsync<Tenant>(apiurl, Tenant);
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant Tenant)
        {
            return await _http.PutJsonAsync<Tenant>(apiurl + "/" + Tenant.TenantId.ToString(), Tenant);
        }

        public async Task DeleteTenantAsync(int TenantId)
        {
            await _http.DeleteAsync(apiurl + "/" + TenantId.ToString());
        }
    }
}
