using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Themes;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Oqtane.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;

namespace Oqtane.Pages
{
    public class HostModel : PageModel
    {
        private IConfiguration _configuration;
        private readonly ITenantManager _tenantManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly ILanguageRepository _languages;
        private readonly IAntiforgery _antiforgery;
        private readonly ISiteRepository _sites;
        private readonly IPageRepository _pages;

        public HostModel(IConfiguration configuration, ITenantManager tenantManager, ILocalizationManager localizationManager, ILanguageRepository languages, IAntiforgery antiforgery, ISiteRepository sites, IPageRepository pages)
        {
            _configuration = configuration;
            _tenantManager = tenantManager;
            _localizationManager = localizationManager;
            _languages = languages;
            _antiforgery = antiforgery;
            _sites = sites;
            _pages = pages;
        }

        public string AntiForgeryToken = "";
        public string Runtime = "Server";
        public RenderMode RenderMode = RenderMode.Server;
        public string HeadResources = "";
        public string BodyResources = "";
        public string Title = "";
        public string FavIcon = "favicon.ico";
        public string PWAScript = "";
        public string ThemeType = "";

        public void OnGet()
        {
            AntiForgeryToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

            if (_configuration.GetSection("Runtime").Exists())
            {
                Runtime = _configuration.GetSection("Runtime").Value;
            }

            if (_configuration.GetSection("RenderMode").Exists())
            {
                RenderMode = (RenderMode)Enum.Parse(typeof(RenderMode), _configuration.GetSection("RenderMode").Value, true);
            }

            // if framework is installed 
            if (!string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
            {
                var alias = _tenantManager.GetAlias();
                if (alias != null)
                {
                    Route route = new Route(HttpContext.Request.GetEncodedUrl(), alias.Path);

                    var site = _sites.GetSite(alias.SiteId);
                    if (site != null)
                    {
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

                            // include theme resources
                            if (!string.IsNullOrEmpty(page.ThemeType))
                            {
                                ThemeType = page.ThemeType;
                            }
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
                    if (HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName] == null)
                    {
                        // set default language for site if the culture is not supported
                        var languages = _languages.GetLanguages(alias.SiteId);
                        if (languages.Any() && languages.All(l => l.Code != CultureInfo.CurrentUICulture.Name))
                        {
                            var defaultLanguage = languages.Where(l => l.IsDefault).SingleOrDefault() ?? languages.First();
                            SetLocalizationCookie(defaultLanguage.Code);
                        }
                        else
                        {
                            SetLocalizationCookie(_localizationManager.GetDefaultCulture());
                        }
                    }
                }
            }
        }

        private string CreatePWAScript(Alias alias, Site site, Route route)
        {
            return
            "<script>" +
                "setTimeout(() => { " +
                    "var manifest = { " +
                        "\"name\": \"" + site.Name + "\", " +
                        "\"short_name\": \"" + site.Name + "\", " +
                        "\"start_url\": \"" + route.Scheme + "://" + route.Authority + "/\", " +
                        "\"display\": \"standalone\", " +
                        "\"background_color\": \"#fff\", " +
                        "\"description\": \"" + site.Name + "\", " +
                        "\"icons\": [{ " +
                            "\"src\": \"" + route.Scheme + "://" + route.Authority + Utilities.ContentUrl(alias, site.PwaAppIconFileId.Value) + "\", " +
                            "\"sizes\": \"192x192\", " +
                            "\"type\": \"image/png\" " +
                            "}, { " +
                            "\"src\": \"" + route.Scheme + "://" + route.Authority + Utilities.ContentUrl(alias, site.PwaSplashIconFileId.Value) + "\", " +
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
                    ProcessResource(resource);
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
                            ProcessResource(resource);
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
                    foreach (var resource in obj.Resources)
                    {
                        if (resource.Declaration == ResourceDeclaration.Global || (Utilities.GetFullTypeName(type.AssemblyQualifiedName) == ThemeType && resource.ResourceType == ResourceType.Stylesheet))
                        {
                            ProcessResource(resource);
                        }
                    }
                }
            }
        }
        private void ProcessResource(Resource resource)
        {
            switch (resource.ResourceType)
            {
                case ResourceType.Stylesheet:
                    if (!HeadResources.Contains(resource.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        var id = (resource.Declaration == ResourceDeclaration.Global) ? "" : "id=\"app-stylesheet-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-00\" ";
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
