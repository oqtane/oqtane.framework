using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ITenantService
    {
        Task<List<Tenant>> GetTenantsAsync();

        Task<Tenant> GetTenantAsync(int TenantId);

        Task<Tenant> AddTenantAsync(Tenant Tenant);

        Task<Tenant> UpdateTenantAsync(Tenant Tenant);

        Task DeleteTenantAsync(int TenantId);
    }
}
