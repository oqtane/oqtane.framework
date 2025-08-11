using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    // class deprecated and replaced by ITenantManager
    public interface ITenantResolver
    {
        Alias GetAlias();
        Tenant GetTenant();
    }

    public class TenantResolver : ITenantResolver
    {
        private readonly ITenantManager _tenantManager;

        public TenantResolver(ITenantManager tenantManager)
        {
            _tenantManager = tenantManager;
        }

        public Alias GetAlias()
        {
            return _tenantManager.GetAlias();
        }

        public Tenant GetTenant()
        {
            return _tenantManager.GetTenant();
        }
    }
}
