using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Themes;
using System;
using System.Linq;
using System.Reflection;
using Oqtane.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net;
using Microsoft.Extensions.Primitives;
using Oqtane.Enums;

namespace Oqtane.Pages
{
    public class HostModel : PageModel
    {
        private IConfigManager _configuration;
        private readonly ITenantManager _tenantManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly ILanguageRepository _languages;
        private readonly IAntiforgery _antiforgery;
        private readonly ISiteRepository _sites;
        private readonly IPageRepository _pages;
        private readonly IUrlMappingRepository _urlMappings;
        private readonly IVisitorRepository _visitors;
        private readonly IAliasRepository _aliases;
        private readonly ISettingRepository _settings;
        private readonly ILogManager _logger;

        public HostModel(IConfigManager configuration, ITenantManager tenantManager, ILocalizationManager localizationManager, ILanguageRepository languages, IAntiforgery antiforgery, ISiteRepository sites, IPageRepository pages, IUrlMappingRepository urlMappings, IVisitorRepository visitors, IAliasRepository aliases, ISettingRepository settings, ILogManager logger)
        {
            _configuration = configuration;
            _tenantManager = tenantManager;
            _localizationManager = localizationManager;
            _languages = languages;
            _antiforgery = antiforgery;
            _sites = sites;
            _pages = pages;
            _urlMappings = urlMappings;
            _visitors = visitors;
            _aliases = aliases;
            _settings = settings;
            _logger = logger;
        }

        public string Language = "en";
        public string AntiForgeryToken = "";
        public string Runtime = "Server";
        public RenderMode RenderMode = RenderMode.Server;
        public int VisitorId = -1;
        public string RemoteIPAddress = "";
        public string HeadResources = "";
        public string BodyResources = "";
        public string Title = "";
        public string Meta = "";
        public string FavIcon = "favicon.ico";
        public string PWAScript = "";
        public string ThemeType = "";
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
                RenderMode = (RenderMode)Enum.Parse(typeof(RenderMode), _configuration.GetSection("RenderMode").Value, true);
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
                    if (site != null && !site.IsDeleted)
                    {
                        Route route = new Route(url, alias.Path);

                        if (!string.IsNullOrEmpty(site.Runtime))
                        {
                            Runtime = site.Runtime;
                        }
                        if (!string.IsNullOrEmpty(site.RenderMode))
                        {
                            RenderMode = (RenderMode)Enum.Parse(typeof(RenderMode), site.RenderMode, true);
                        }
                        if (site.FaviconFileId != null)
                        {
                            FavIcon = Utilities.ContentUrl(alias, site.FaviconFileId.Value);
                        }
                        if (site.PwaIsEnabled && site.PwaAppIconFileId != null && site.PwaSplashIconFileId != null)
                        {
                            PWAScript = CreatePWAScript(alias, site, route);
                        }
                        Title = site.Name;
                        ThemeType = site.DefaultThemeType;

                        if (site.VisitorTracking)
                        {
                            TrackVisitor(site.SiteId);
                        }

                        var page = _pages.GetPage(route.PagePath, site.SiteId);
                        if (page != null)
                        {
                            // set page title
                            if (!string.IsNullOrEmpty(page.Title))
                            {
                                Title = page.Title;
                            }
                            else
                            {
                                Title = Title + " - " + page.Name;
                            }
                            Meta = page.Meta;

                            // include theme resources
                            if (!string.IsNullOrEmpty(page.ThemeType))
                            {
                                ThemeType = page.ThemeType;
                            }
                        }
                        else // page not found
                        {
                            // look for url mapping
                            var urlMapping = _urlMappings.GetUrlMapping(site.SiteId, route.PagePath);
                            if (urlMapping != null && !string.IsNullOrEmpty(urlMapping.MappedUrl))
                            {
                                url = (urlMapping.MappedUrl.StartsWith("http")) ? urlMapping.MappedUrl : route.SiteUrl + "/" + urlMapping.MappedUrl;
                                return RedirectPermanent(url);
                            }
                        }

                        // include global resources
                        var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
                        foreach (Assembly assembly in assemblies)
                        {
                            ProcessHostResources(assembly);
                            ProcessModuleControls(assembly);
                            ProcessThemeControls(assembly);
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
                        Message = "Site Is Either Disabled Or Not Configured Correctly";
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
                if (User.HasClaim(item => item.Type == ClaimTypes.PrimarySid))
                {
                    userid = int.Parse(User.Claims.First(item => item.Type == ClaimTypes.PrimarySid).Value);
                }

                // check if cookie already exists
                Visitor visitor = null;
                bool addcookie = false;
                var VisitorCookie = "APP_VISITOR_" + SiteId.ToString();
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
            "<script id=\"app-pwa\">" +
                "setTimeout(() => { " +
                    "var manifest = { " +
                        "\"name\": \"" + site.Name + "\", " +
                        "\"short_name\": \"" + site.Name + "\", " +
                        "\"start_url\": \"" + route.SiteUrl + "/\", " +
                        "\"display\": \"standalone\", " +
                        "\"background_color\": \"#fff\", " +
                        "\"description\": \"" + site.Name + "\", " +
                        "\"icons\": [{ " +
                            "\"src\": \"" + route.RootUrl + Utilities.ContentUrl(alias, site.PwaAppIconFileId.Value) + "\", " +
                            "\"sizes\": \"192x192\", " +
                            "\"type\": \"image/png\" " +
                            "}, { " +
                            "\"src\": \"" + route.RootUrl + Utilities.ContentUrl(alias, site.PwaSplashIconFileId.Value) + "\", " +
                            "\"sizes\": \"512x512\", " +
                            "\"type\": \"image/png\" " +
                        "}] " +
                    "}; " +
                    "const serialized = JSON.stringify(manifest); " +
                    "const blob = new Blob([serialized], {type: 'application/javascript'}); " +
                    "const url = URL.createObjectURL(blob); " +
                    "document.getElementById('app-manifest').setAttribute('href', url); " +
                "} " +
                ", 1000);" +
            "</script>" + Environment.NewLine +
            "<script id=\"app-serviceworker\">" +
                "if ('serviceWorker' in navigator) { " +
                    "navigator.serviceWorker.register('/service-worker.js').then(function(registration) { " +
                        "console.log('ServiceWorker Registration Successful'); " +
                    "}).catch (function(err) { " +
                        "console.log('ServiceWorker Registration Failed ', err); " +
                    "}); " +
                "};" +
            "</script>";
        }

        private void ProcessHostResources(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IHostResources)));
            foreach (var type in types)
            {
                var obj = Activator.CreateInstance(type) as IHostResources;
                foreach (var resource in obj.Resources)
                {
                    resource.Declaration = ResourceDeclaration.Global;
                    ProcessResource(resource, 0);
                }
            }
        }

        private void ProcessModuleControls(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IModuleControl)));
            foreach (var type in types)
            {
                // Check if type should be ignored
                if (type.IsOqtaneIgnore()) continue;

                var obj = Activator.CreateInstance(type) as IModuleControl;
                if (obj.Resources != null)
                {
                    foreach (var resource in obj.Resources)
                    {
                        if (resource.Declaration == ResourceDeclaration.Global)
                        {
                            ProcessResource(resource, 0);
                        }
                    }
                }
            }
        }

        private void ProcessThemeControls(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IThemeControl)));
            foreach (var type in types)
            {
                // Check if type should be ignored
                if (type.IsOqtaneIgnore()) continue;

                var obj = Activator.CreateInstance(type) as IThemeControl;
                if (obj.Resources != null)
                {
                    int count = 0; // required for local stylesheets for current theme
                    foreach (var resource in obj.Resources)
                    {
                        if (resource.Declaration == ResourceDeclaration.Global || (Utilities.GetFullTypeName(type.AssemblyQualifiedName) == ThemeType && resource.ResourceType == ResourceType.Stylesheet))
                        {
                            ProcessResource(resource, count++);
                        }
                    }
                }
            }
        }
        private void ProcessResource(Resource resource, int count)
        {
            switch (resource.ResourceType)
            {
                case ResourceType.Stylesheet:
                    if (!HeadResources.Contains(resource.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        var id = (resource.Declaration == ResourceDeclaration.Global) ? "" : "id=\"app-stylesheet-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + count.ToString("00") + "\" ";
                        HeadResources += "<link " + id + "rel=\"stylesheet\" href=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + " />" + Environment.NewLine;
                    }
                    break;
                case ResourceType.Script:
                    if (resource.Location == Shared.ResourceLocation.Body)
                    {
                        if (!BodyResources.Contains(resource.Url, StringComparison.OrdinalIgnoreCase))
                        {
                            BodyResources += "<script src=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + "></script>" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        if (!HeadResources.Contains(resource.Url, StringComparison.OrdinalIgnoreCase))
                        {
                            HeadResources += "<script src=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + "></script>" + Environment.NewLine;
                        }
                    }
                    break;
            }
        }
        private string CrossOrigin(string crossorigin)
        {
            if (!string.IsNullOrEmpty(crossorigin))
            {
                return " crossorigin=\"" + crossorigin + "\"";
            }
            else
            {
                return "";
            }
        }
        private string Integrity(string integrity)
        {
            if (!string.IsNullOrEmpty(integrity))
            {
                return " integrity=\"" + integrity + "\"";
            }
            else
            {
                return "";
            }
        }

        private void SetLocalizationCookie(string culture)
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
        }
    }
}
