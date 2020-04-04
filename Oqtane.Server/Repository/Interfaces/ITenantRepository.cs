using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetTenants();
        Tenant AddTenant(Tenant tenant);
        Tenant UpdateTenant(Tenant tenant);
        Tenant GetTenant(int tenantId);
        void DeleteTenant(int tenantId);
    }
}
