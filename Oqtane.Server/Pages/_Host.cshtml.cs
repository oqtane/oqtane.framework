using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Models;
using System;
using System.Linq;
using Oqtane.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net;
using Microsoft.Extensions.Primitives;
using Oqtane.Enums;
using Oqtane.Security;
using Oqtane.Extensions;
using Oqtane.Themes;
using System.Collections.Generic;

namespace Oqtane.Pages
{
    public class HostModel : PageModel
    {
        private IConfigManager _configuration;
        private readonly ITenantManager _tenantManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly ILanguageRepository _languages;
        private readonly IAntiforgery _antiforgery;
        private readonly IJwtManager _jwtManager;
        private readonly ISiteRepository _sites;
        private readonly IPageRepository _pages;
        private readonly IUrlMappingRepository _urlMappings;
        private readonly IVisitorRepository _visitors;
        private readonly IAliasRepository _aliases;
        private readonly ISettingRepository _settings;
        private readonly IThemeRepository _themes;
        private readonly IServerStateManager _serverState;
        private readonly ILogManager _logger;

        public HostModel(IConfigManager configuration, ITenantManager tenantManager, ILocalizationManager localizationManager, ILanguageRepository languages, IAntiforgery antiforgery, IJwtManager jwtManager, ISiteRepository sites, IPageRepository pages, IUrlMappingRepository urlMappings, IVisitorRepository visitors, IAliasRepository aliases, ISettingRepository settings, IThemeRepository themes, IServerStateManager serverState, ILogManager logger)
        {
            _configuration = configuration;
            _tenantManager = tenantManager;
            _localizationManager = localizationManager;
            _languages = languages;
            _antiforgery = antiforgery;
            _jwtManager = jwtManager;
            _sites = sites;
            _pages = pages;
            _urlMappings = urlMappings;
            _visitors = visitors;
            _aliases = aliases;
            _settings = settings;
            _themes = themes;
            _serverState = serverState;
            _logger = logger;
        }

        public string Language = "en";
        public string AntiForgeryToken = "";
        public string AuthorizationToken = "";
        public string Runtime = "Server";
        public string RenderMode = "ServerPrerendered";
        public int VisitorId = -1;
        public string RemoteIPAddress = "";
        public string HeadResources = "";
        public string BodyResources = "";
        public string StyleSheets = "";
        public string PWAScript = "";
        public string ReconnectScript = "";
        public string Message = "";

        public IActionResult OnGet()
        {
            AntiForgeryToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
            RemoteIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            if (_configuration.GetSection("Runtime").Exists())
            {
                Runtime = _configuration.GetSection("Runtime").Value;
            }

            if (_configuration.GetSection("RenderMode").Exists())
            {
                RenderMode = _configuration.GetSection("RenderMode").Value;
            }

            // if framework is installed
            if (_configuration.IsInstalled())
            {
                var alias = _tenantManager.GetAlias();
                if (alias != null)
                {
                    var url = WebUtility.UrlDecode(HttpContext.Request.GetEncodedUrl());

                    // redirect non-default alias unless you are trying to access site settings
                    if (!alias.IsDefault && !url.Contains("admin/site"))
                    {
                        var aliases = _aliases.GetAliases().Where(item => item.TenantId == alias.TenantId && item.SiteId == alias.SiteId);
                        if (aliases.Where(item => item.IsDefault).FirstOrDefault() != null)
                        {
                            return RedirectPermanent(url.Replace(alias.Name, aliases.Where(item => item.IsDefault).FirstOrDefault().Name));
                        }
                        else // no default specified - use first alias
                        {
                            if (alias.Name.Trim() != aliases.First().Name.Trim())
                            {
                                return RedirectPermanent(url.Replace(alias.Name, aliases.First().Name));
                            }
                        }
                    }

                    var site = _sites.GetSite(alias.SiteId);
                    if (site != null && (!site.IsDeleted || url.Contains("admin/site")) && site.Runtime != "Hybrid")
                    {
                        Route route = new Route(url, alias.Path);

                        if (!string.IsNullOrEmpty(site.Runtime))
                        {
                            Runtime = site.Runtime;
                        }
                       if (!string.IsNullOrEmpty(site.RenderMode))
                        {
                            RenderMode = site.RenderMode;
                        }
 
                        var page = _pages.GetPage(route.PagePath, site.SiteId);
                        if (page == null && route.PagePath == "" && site.HomePageId != null)
                        {
                            page = _pages.GetPage(site.HomePageId.Value);
                        }
                        if (page == null || page.IsDeleted)
                        {
                            // page not found - look for url mapping
                            var urlMapping = _urlMappings.GetUrlMapping(site.SiteId, route.PagePath);
                            if (urlMapping != null && !string.IsNullOrEmpty(urlMapping.MappedUrl))
                            {
                                url = (urlMapping.MappedUrl.StartsWith("http")) ? urlMapping.MappedUrl : route.SiteUrl + "/" + urlMapping.MappedUrl;
                                return RedirectPermanent(url);
                            }
                            else
                            {
                                if (route.PagePath != "404")
                                {
                                    return RedirectPermanent(route.SiteUrl + "/404");
                                }
                            }
                        }

                        if (site.VisitorTracking)
                        {
                            TrackVisitor(site.SiteId);
                        }

                        // get jwt token for downstream APIs
                        if (User.Identity.IsAuthenticated)
                        {
                            var sitesettings = HttpContext.GetSiteSettings();
                            var secret = sitesettings.GetValue("JwtOptions:Secret", "");
                            if (!string.IsNullOrEmpty(secret))
                            {
                                AuthorizationToken = _jwtManager.GenerateToken(alias, (ClaimsIdentity)User.Identity, secret, sitesettings.GetValue("JwtOptions:Issuer", ""), sitesettings.GetValue("JwtOptions:Audience", ""), int.Parse(sitesettings.GetValue("JwtOptions:Lifetime", "20")));
                            }
                        }

                        // stylesheets
                        var themes = _themes.GetThemes().ToList();
                        var resources = new List<Resource>();
                        if (string.IsNullOrEmpty(page.ThemeType))
                        {
                            page.ThemeType = site.DefaultThemeType;
                        }
                        var theme = themes.FirstOrDefault(item => item.Themes.Any(item => item.TypeName == page.ThemeType));
                        if (theme?.Resources != null)
                        {
                            resources.AddRange(theme.Resources.Where(item => item.ResourceType == ResourceType.Stylesheet).ToList());
                        }
                        var type = Type.GetType(page.ThemeType);
                        if (type != null)
                        {
                            var obj = Activator.CreateInstance(type) as IThemeControl;
                            if (obj?.Resources != null)
                            {
                                resources.AddRange(obj.Resources.Where(item => item.ResourceType == ResourceType.Stylesheet).ToList());
                            }
                        }
                        ManageStyleSheets(resources, alias, theme.ThemeName);

                        // scripts
                        if (Runtime == "Server")
                        {
                            ReconnectScript = CreateReconnectScript();
                        }
                        if (site.PwaIsEnabled && site.PwaAppIconFileId != null && site.PwaSplashIconFileId != null)
                        {
                            PWAScript = CreatePWAScript(alias, site, route);
                        }
                        HeadResources += ParseScripts(site.HeadContent);
                        BodyResources += ParseScripts(site.BodyContent);
                        var scripts = _serverState.GetServerState(alias.SiteKey).Scripts;
                        foreach (var script in scripts)
                        {
                            AddScript(script, alias);
                        }

                        // set culture if not specified
                        string culture = HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
                        if (culture == null)
                        {
                            // get default language for site
                            var languages = _languages.GetLanguages(alias.SiteId);
                            if (languages.Any())
                            {
                                // use default language if specified otherwise use first language in collection
                                culture = (languages.Where(l => l.IsDefault).SingleOrDefault() ?? languages.First()).Code;
                            }
                            else
                            {
                                culture = _localizationManager.GetDefaultCulture();
                            }
                            SetLocalizationCookie(culture);
                        }

                        // set language for page
                        if (!string.IsNullOrEmpty(culture))
                        {
                            // localization cookie value in form of c=en|uic=en
                            Language = culture.Split('|')[0];
                            Language = Language.Replace("c=", "");
                        }
                    }
                    else
                    {
                        Message = "Site Is Disabled";
                    }
                }
                else
                {
                    Message = "Site Not Configured Correctly - No Matching Alias Exists For Host Name";
                }
            }
            return Page();
        }

        private void TrackVisitor(int SiteId)
        {
            try
            {
                // get request attributes
                string useragent = (Request.Headers[HeaderNames.UserAgent] != StringValues.Empty) ? Request.Headers[HeaderNames.UserAgent] : "(none)";
                useragent = (useragent.Length > 256) ? useragent.Substring(0, 256) : useragent;
                string language = (Request.Headers[HeaderNames.AcceptLanguage] != StringValues.Empty) ? Request.Headers[HeaderNames.AcceptLanguage] : "";
                language = (language.Contains(",")) ? language.Substring(0, language.IndexOf(",")) : language;
                language = (language.Contains(";")) ? language.Substring(0, language.IndexOf(";")) : language;
                language = (language.Trim().Length == 0) ? "??" : language;

                // filter
                string filter = Constants.DefaultVisitorFilter;
                var settings = _settings.GetSettings(EntityNames.Site, SiteId);
                if (settings.Any(item => item.SettingName == "VisitorFilter"))
                {
                    filter = settings.First(item => item.SettingName == "VisitorFilter").SettingValue;
                }
                foreach (string term in filter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(sValue => sValue.Trim()).ToArray())
                {
                    if (RemoteIPAddress.ToLower().Contains(term) || useragent.ToLower().Contains(term) || language.ToLower().Contains(term))
                    {
                        return;
                    }
                }

                // get other request attributes
                string url = Request.GetEncodedUrl();
                string referrer = (Request.Headers[HeaderNames.Referer] != StringValues.Empty) ? Request.Headers[HeaderNames.Referer] : "";
                int? userid = null;
                if (User.HasClaim(item => item.Type == ClaimTypes.NameIdentifier))
                {
                    userid = int.Parse(User.Claims.First(item => item.Type == ClaimTypes.NameIdentifier).Value);
                }

                // check if cookie already exists
                Visitor visitor = null;
                bool addcookie = false;
                var VisitorCookie = Constants.VisitorCookiePrefix + SiteId.ToString();
                if (!int.TryParse(Request.Cookies[VisitorCookie], out VisitorId))
                {
                    // if enabled use IP Address correlation
                    VisitorId = -1;
                    bool correlate = true;
                    if (settings.Any(item => item.SettingName == "VisitorCorrelation"))
                    {
                        correlate = bool.Parse(settings.First(item => item.SettingName == "VisitorCorrelation").SettingValue);
                    }
                    if (correlate)
                    {
                        visitor = _visitors.GetVisitor(SiteId, RemoteIPAddress);
                        if (visitor != null)
                        {
                            VisitorId = visitor.VisitorId;
                            addcookie = true;
                        }
                    }
                }

                if (VisitorId == -1)
                {
                    // create new visitor
                    visitor = new Visitor();
                    visitor.SiteId = SiteId;
                    visitor.IPAddress = RemoteIPAddress;
                    visitor.UserAgent = useragent;
                    visitor.Language = language;
                    visitor.Url = url;
                    visitor.Referrer = referrer;
                    visitor.UserId = userid;
                    visitor.Visits = 1;
                    visitor.CreatedOn = DateTime.UtcNow;
                    visitor.VisitedOn = DateTime.UtcNow;
                    visitor = _visitors.AddVisitor(visitor);
                    VisitorId = visitor.VisitorId;
                    addcookie = true;
                }
                else
                {
                    if (visitor == null)
                    {
                        // get visitor if it was not previously loaded
                        visitor = _visitors.GetVisitor(VisitorId);
                    }
                    if (visitor != null)
                    {
                        // update visitor
                        visitor.IPAddress = RemoteIPAddress;
                        visitor.UserAgent = useragent;
                        visitor.Language = language;
                        visitor.Url = url;
                        if (!string.IsNullOrEmpty(referrer))
                        {
                            visitor.Referrer = referrer;
                        }
                        if (userid != null)
                        {
                            visitor.UserId = userid;
                        }
                        visitor.Visits += 1;
                        visitor.VisitedOn = DateTime.UtcNow;
                        _visitors.UpdateVisitor(visitor);
                    }
                    else
                    {
                        // remove cookie if VisitorId does not exist
                        Response.Cookies.Delete(VisitorCookie);
                    }
                }

                // append cookie
                if (addcookie)
                {
                    Response.Cookies.Append(
                        VisitorCookie,
                        VisitorId.ToString(),
                        new CookieOptions()
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            IsEssential = true
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Tracking Visitor {Error}", ex.Message);
            }
        }

        private string CreatePWAScript(Alias alias, Site site, Route route)
        {
            return
            "<script>" + Environment.NewLine +
            "    // PWA Manifest" + Environment.NewLine +
            "    setTimeout(() => {" + Environment.NewLine +
            "        var manifest = {" + Environment.NewLine +
            "            \"name\": \"" + site.Name + "\"," + Environment.NewLine +
            "            \"short_name\": \"" + site.Name + "\"," + Environment.NewLine +
            "            \"start_url\": \"" + route.SiteUrl + "/\"," + Environment.NewLine +
            "            \"display\": \"standalone\"," + Environment.NewLine +
            "            \"background_color\": \"#fff\"," + Environment.NewLine +
            "            \"description\": \"" + site.Name + "\"," + Environment.NewLine +
            "            \"icons\": [{" + Environment.NewLine +
            "                \"src\": \"" + route.RootUrl + Utilities.FileUrl(alias, site.PwaAppIconFileId.Value) + "\"," + Environment.NewLine +
            "                \"sizes\": \"192x192\"," + Environment.NewLine +
            "                \"type\": \"image/png\"" + Environment.NewLine +
            "                }, {" + Environment.NewLine +
            "                \"src\": \"" + route.RootUrl + Utilities.FileUrl(alias, site.PwaSplashIconFileId.Value) + "\"," + Environment.NewLine +
            "                \"sizes\": \"512x512\"," + Environment.NewLine +
            "                \"type\": \"image/png\"" + Environment.NewLine +
            "            }]" + Environment.NewLine +
            "       };" + Environment.NewLine +
            "       const serialized = JSON.stringify(manifest);" + Environment.NewLine +
            "       const blob = new Blob([serialized], {type: 'application/javascript'});" + Environment.NewLine +
            "       const url = URL.createObjectURL(blob);" + Environment.NewLine +
            "       document.getElementById('app-manifest').setAttribute('href', url);" + Environment.NewLine +
            "    }, 1000);" + Environment.NewLine +
            "</script>" + Environment.NewLine +
            "<script>" + Environment.NewLine +
            "    // PWA Service Worker" + Environment.NewLine +
            "    if ('serviceWorker' in navigator) {" + Environment.NewLine +
            "        navigator.serviceWorker.register('/service-worker.js').then(function(registration) {" + Environment.NewLine +
            "            console.log('ServiceWorker Registration Successful');" + Environment.NewLine +
            "        }).catch (function(err) {" + Environment.NewLine +
            "            console.log('ServiceWorker Registration Failed ', err);" + Environment.NewLine +
            "        });" + Environment.NewLine +
            "    };" + Environment.NewLine +
            "</script>";
        }

        private string CreateReconnectScript()
        {
            return
            "<script>" + Environment.NewLine +
            "    // Blazor Server Reconnect" + Environment.NewLine +
            "    new MutationObserver((mutations, observer) => {" + Environment.NewLine +
            "        if (document.querySelector('#components-reconnect-modal h5 a')) {" + Environment.NewLine +
            "            async function attemptReload() {" + Environment.NewLine +
            "                await fetch('');" + Environment.NewLine +
            "                location.reload();" + Environment.NewLine +
            "            }" + Environment.NewLine +
            "            observer.disconnect();" + Environment.NewLine +
            "            attemptReload();" + Environment.NewLine +
            "            setInterval(attemptReload, 5000);" + Environment.NewLine +
            "        }" + Environment.NewLine +
            "    }).observe(document.body, { childList: true, subtree: true });" + Environment.NewLine +
            "</script>";
        }

        private string ParseScripts(string content)
        {
            // iterate scripts
            var scripts = "";
            if (!string.IsNullOrEmpty(content))
            {
                var index = content.IndexOf("<script");
                while (index >= 0)
                {
                    scripts += content.Substring(index, content.IndexOf("</script>", index) + 9 - index);
                    index = content.IndexOf("<script", index + 1);
                }
            }
            return scripts;
        }

        private void AddScript(Resource resource, Alias alias)
        {
            var script = CreateScript(resource, alias);
            if (resource.Location == Shared.ResourceLocation.Head)
            {
                if (!HeadResources.Contains(script))
                {
                    HeadResources += script + Environment.NewLine;
                }
            }
            else
            {
                if (!BodyResources.Contains(script))
                {
                    BodyResources += script + Environment.NewLine;
                }
            }
        }

        private string CreateScript(Resource resource, Alias alias)
        {
            if (!string.IsNullOrEmpty(resource.Url))
            {
                var url = (resource.Url.Contains("://")) ? resource.Url : alias.BaseUrl + resource.Url;
                return "<script src=\"" + url + "\"" +
                    ((!string.IsNullOrEmpty(resource.Integrity)) ? " integrity=\"" + resource.Integrity + "\"" : "") +
                    ((!string.IsNullOrEmpty(resource.CrossOrigin)) ? " crossorigin=\"" + resource.CrossOrigin + "\"" : "") +
                    "></script>";
            }
            else
            {
                // inline script
                return "<script>" + resource.Content + "</script>";
            }
        }

        private void SetLocalizationCookie(string culture)
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
        }

        private void ManageStyleSheets(List<Resource> resources, Alias alias, string name)
        {
            if (resources != null)
            {
                int count = 0;
                foreach (var resource in resources)
                {
                    if (resource.Url.StartsWith("~"))
                    {
                        resource.Url = resource.Url.Replace("~", "/Themes/" + Utilities.GetTypeName(name) + "/").Replace("//", "/");
                    }
                    if (!resource.Url.Contains("://") && alias.BaseUrl != "" && !resource.Url.StartsWith(alias.BaseUrl))
                    {
                        resource.Url = alias.BaseUrl + resource.Url;
                    }

                    if (!StyleSheets.Contains(resource.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        string id = "id=\"app-stylesheet-" + ResourceLevel.Page.ToString().ToLower() + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + count.ToString("00") + "\" ";
                        StyleSheets += "<link " + id + "rel=\"stylesheet\" href=\"" + resource.Url + "\"" + (!string.IsNullOrEmpty(resource.Integrity) ? " integrity=\"" + resource.Integrity + "\"" : "") + (!string.IsNullOrEmpty(resource.CrossOrigin) ? " crossorigin=\"" + resource.CrossOrigin + "\"" : "") + " type=\"text/css\"/>" + Environment.NewLine;
                    }
                }
            }
        }
    }
}
