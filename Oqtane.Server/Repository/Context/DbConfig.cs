using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Oqtane.Repository
{
    public class DbConfig : IDbConfig
    {
        public DbConfig(IHttpContextAccessor accessor, IConfiguration configuration)
        {
            Accessor = accessor;
            Configuration = configuration;
        }

        public IHttpContextAccessor Accessor { get; }

        public IConfiguration Configuration { get; }

        public string ConnectionString { get; set; }
    }
}
