using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Repository
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetTenants();
        void AddTenant(Tenant tenant);
        void UpdateTenant(Tenant tenant);
        Tenant GetTenant(int tenantId);
        void DeleteTenant(int tenantId);
    }
}
