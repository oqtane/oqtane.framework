using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Oqtane.Repository
{
    public interface IDbConfig
    {
        public IHttpContextAccessor Accessor { get; }

        public IConfiguration Configuration { get; }

        public string ConnectionString { get; set; }
    }
}
