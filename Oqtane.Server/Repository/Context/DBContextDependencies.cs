using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oqtane.Infrastructure;

namespace Oqtane.Repository
{
    public class DBContextDependencies : IDBContextDependencies
    {
        public DBContextDependencies(ITenantManager tenantManager, IHttpContextAccessor httpContextAccessor, IConfigurationRoot config)
        {
            TenantManager = tenantManager;
            Accessor = httpContextAccessor;
            Config = config;
        }

        public ITenantManager TenantManager { get; }
        public IHttpContextAccessor Accessor { get; }
        public IConfigurationRoot Config { get; }
    }
}
