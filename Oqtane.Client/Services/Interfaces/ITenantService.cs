using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
}
