using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ITenantManager
    {
        Alias GetAlias();
        Tenant GetTenant();
        void SetAlias(Alias alias);
        void SetTenant(int tenantId);
    }
}
