using Oqtane.Models;

namespace Oqtane.Repository
{
    // class deprecated and replaced by ITenantManager
    public interface ITenantResolver
    {
        Alias GetAlias();
        Tenant GetTenant();
    }
}
