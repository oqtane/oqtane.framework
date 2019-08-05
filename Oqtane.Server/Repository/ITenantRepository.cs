using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Repository
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetTenants();
        Tenant AddTenant(Tenant Tenant);
        Tenant UpdateTenant(Tenant Tenant);
        Tenant GetTenant(int TenantId);
        void DeleteTenant(int TenantId);
    }
}
