using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ITenantService
    {
        Task<List<Tenant>> GetTenantsAsync();

        Task<Tenant> GetTenantAsync();
    }
}
