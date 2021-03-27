using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oqtane.Interfaces;

namespace Oqtane.Repository
{
    public class DbConfig : IDbConfig
    {
        public DbConfig(IHttpContextAccessor accessor, IConfiguration configuration, IEnumerable<IOqtaneDatabase> databases)
        {
            Accessor = accessor;
            Configuration = configuration;
            Databases = databases;
        }

        public IHttpContextAccessor Accessor { get; }

        public IConfiguration Configuration { get; }

        public IEnumerable<IOqtaneDatabase> Databases { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseType { get; set; }
    }
}
