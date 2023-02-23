using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oqtane.Infrastructure;

namespace Oqtane.Repository
{
    public interface IDBContextDependencies
    {
        ITenantManager TenantManager { get; }
        IHttpContextAccessor Accessor { get; }
        IConfigurationRoot Config { get; }
    }
}
