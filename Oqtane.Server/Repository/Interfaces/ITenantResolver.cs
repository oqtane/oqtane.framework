using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ITenantResolver
    {
        Tenant GetTenant();
    }
}
