using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Repository;
using Oqtane.Shared;

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
            var config = context.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
            string path = context.Request.Path.ToString();

            if (config.IsInstalled() && !path.StartsWith("/_blazor"))
            {
                // get alias (note that this also sets SiteState.Alias)
                var tenantManager = context.RequestServices.GetService(typeof(ITenantManager)) as ITenantManager;
                var alias = tenantManager.GetAlias();

                if (alias != null)
                {
                    // add site settings to alias
                    var cache = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
                    alias.SiteSettings = cache.GetOrCreate("sitesettings:" + alias.SiteKey, entry =>
                    {
                        var settingRepository = context.RequestServices.GetService(typeof(ISettingRepository)) as ISettingRepository;
                        return settingRepository.GetSettings(EntityNames.Site, alias.SiteId)
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                    });
                    // save alias in HttpContext for server-side usage
                    context.Items.Add(Constants.HttpContextAliasKey, alias);
                    // remove site settings so they are not available client-side
                    alias.SiteSettings = null;

                    // rewrite path by removing alias path prefix from api and pages requests (for consistent routing)
                    if (!string.IsNullOrEmpty(alias.Path))
                    {
                        if (path.StartsWith("/" + alias.Path) && (path.Contains("/api/") || path.Contains("/pages/")))
                        {
                            context.Request.Path = path.Replace("/" + alias.Path, "");
                        }
                    }
                }
            }

            // continue processing
            if (next != null) await next(context);
        }
    }
}
