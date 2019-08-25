using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ITenantResolver
    {
        Alias GetAlias();
        Tenant GetTenant();
    }
}
