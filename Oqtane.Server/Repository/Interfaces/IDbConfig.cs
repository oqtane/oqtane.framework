using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oqtane.Interfaces;

namespace Oqtane.Repository
{
    public interface IDbConfig
    {
        public IHttpContextAccessor Accessor { get; }

        public IConfiguration Configuration { get; }

        public IEnumerable<IOqtaneDatabase> Databases { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseType { get; set; }
    }
}
