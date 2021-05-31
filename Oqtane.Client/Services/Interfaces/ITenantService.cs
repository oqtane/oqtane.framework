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

        /// <summary>
        /// Add / save another <see cref="Tenant"/> to the database
        /// </summary>
        /// <param name="tenant">A <see cref="Tenant"/> object containing the configuration</param>
        /// <returns></returns>
        Task<Tenant> AddTenantAsync(Tenant tenant);

        /// <summary>
        /// Update the <see cref="Tenant"/> information in the database.
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        Task<Tenant> UpdateTenantAsync(Tenant tenant);

        /// <summary>
        /// Delete / remove a <see cref="Tenant"/>
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task DeleteTenantAsync(int tenantId);
    }
}
