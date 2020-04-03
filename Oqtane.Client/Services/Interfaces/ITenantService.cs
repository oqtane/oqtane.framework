using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface ITenantService
    {
        Task<List<Tenant>> GetTenantsAsync();

        Task<Tenant> GetTenantAsync(int tenantId);

        Task<Tenant> AddTenantAsync(Tenant tenant);

        Task<Tenant> UpdateTenantAsync(Tenant tenant);

        Task DeleteTenantAsync(int tenantId);
    }
}
