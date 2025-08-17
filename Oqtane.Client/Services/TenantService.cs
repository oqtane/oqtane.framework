using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Tenant"/>s on the Oqtane installation.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Get all <see cref="Tenant"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<Tenant>> GetTenantsAsync();

        /// <summary>
        /// Get one specific <see cref="Tenant"/>
        /// </summary>
        /// <param name="tenantId">ID-reference of the <see cref="Tenant"/></param>
        /// <returns></returns>
        Task<Tenant> GetTenantAsync(int tenantId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class TenantService : ServiceBase, ITenantService
    {
        public TenantService(HttpClient http, SiteState siteState) : base(http, siteState) { }

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
    }
}
