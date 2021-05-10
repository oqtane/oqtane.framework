using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Oqtane.Infrastructure
{
    internal class TenantMiddleware
    {
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if framework is installed
            var config = context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            if (!string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")))
            {
                // get alias
                var tenantManager = context.RequestServices.GetService(typeof(ITenantManager)) as ITenantManager;
                var alias = tenantManager.GetAlias();

                // rewrite path by removing alias path prefix from api and pages requests
                if (alias != null && !string.IsNullOrEmpty(alias.Path))
                {
                    string path = context.Request.Path.ToString();
                    if (path.StartsWith("/" + alias.Path) && (path.Contains("/api/") || path.Contains("/pages/")))
                    {
                        context.Request.Path = path.Replace("/" + alias.Path, "");
                    }
                }
            }

            // continue processing
            if (next != null) await next(context);
        }
    }
}
