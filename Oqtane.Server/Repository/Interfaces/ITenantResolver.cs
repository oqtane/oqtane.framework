using Oqtane.Models;

namespace Oqtane.Repository.Interfaces
{
    public interface ITenantResolver
    {
        Alias GetAlias();
        Tenant GetTenant();
    }
}
