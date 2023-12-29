using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Repository;
using Oqtane.Shared;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Oqtane.Infrastructure
{
    internal class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if framework is installed
            var config = context.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
            string path = context.Request.Path.ToString();

            if (config.IsInstalled() && !path.StartsWith("/_")) // ignore Blazor framework requests
            {
                // get alias (note that this also sets SiteState.Alias)
                var tenantManager = context.RequestServices.GetService(typeof(ITenantManager)) as ITenantManager;
                var alias = tenantManager.GetAlias();

                if (alias != null)
                {
                    // save alias in HttpContext
                    context.Items.Add(Constants.HttpContextAliasKey, alias);

                    // save site settings in HttpContext
                    var cache = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
                    var sitesettings = cache.GetOrCreate(Constants.HttpContextSiteSettingsKey + alias.SiteKey, entry =>
                    {
                        var settingRepository = context.RequestServices.GetService(typeof(ISettingRepository)) as ISettingRepository;
                        return settingRepository.GetSettings(EntityNames.Site, alias.SiteId)
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                    });
                    context.Items.Add(Constants.HttpContextSiteSettingsKey, sitesettings);

                    // handle first request to site
                    var serverState = context.RequestServices.GetService(typeof(IServerStateManager)) as IServerStateManager;
                    if (!serverState.GetServerState(alias.SiteKey).IsInitialized)
                    {
                        var sites = context.RequestServices.GetService(typeof(ISiteRepository)) as ISiteRepository;
                        sites.InitializeSite(alias);
                    }

                    // rewrite path by removing alias path prefix from reserved route (api,pages,files) requests for consistent routes
                    if (!string.IsNullOrEmpty(alias.Path))
                    {
                        if (path.StartsWith("/" + alias.Path) && (Constants.ReservedRoutes.Any(item => path.Contains("/" + item + "/"))))
                        {
                            context.Request.Path = path.Substring(alias.Path.Length + 1);
                        }
                    }

                    // handle sitemap.xml request
                    if (context.Request.Path.ToString().Contains("/sitemap.xml") && !context.Request.Path.ToString().Contains("/pages"))
                    {
                        context.Request.Path = "/pages/sitemap.xml";
                    }

                    // handle robots.txt root request (does not support subfolder aliases)
                    if (context.Request.Path.StartsWithSegments("/robots.txt") && string.IsNullOrEmpty(alias.Path))
                    {
                        // allow all user agents and specify site map
                        var robots = $"User-agent: *\n\nSitemap: {context.Request.Scheme}://{alias.Name}/sitemap.xml";
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(robots);
                        return;
                    }
                }
            }

            // continue processing
            if (_next != null) await _next(context);
        }
    }
}
