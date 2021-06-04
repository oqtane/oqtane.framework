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

namespace Oqtane.Pages
{
    public class HostModel : PageModel
    {
        private IConfiguration _configuration;
        private readonly ITenantManager _tenantManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly ILanguageRepository _languages;

        public HostModel(
            IConfiguration configuration,
            ITenantManager tenantManager,
            ILocalizationManager localizationManager,
            ILanguageRepository languages)
        {
            _configuration = configuration;
            _tenantManager = tenantManager;
            _localizationManager = localizationManager;
            _languages = languages;
        }

        public string Runtime = "Server";
        public RenderMode RenderMode = RenderMode.Server;
        public string HeadResources = "";
        public string BodyResources = "";
        public string Message = "";

        public void OnGet()
        {
            if (_configuration.GetSection("Runtime").Exists())
            {
                Runtime = _configuration.GetSection("Runtime").Value;
            }

            if (Runtime != "WebAssembly" && _configuration.GetSection("RenderMode").Exists())
            {
                RenderMode = (RenderMode)Enum.Parse(typeof(RenderMode), _configuration.GetSection("RenderMode").Value, true);
            }

            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                ProcessHostResources(assembly);
                ProcessModuleControls(assembly);
                ProcessThemeControls(assembly);
            }

            // if culture not specified and framework is installed 
            if (!string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
            {
                var alias = _tenantManager.GetAlias();
                if (alias != null)
                {
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
                        if (resource.Declaration == ResourceDeclaration.Global)
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
                        HeadResources += "<link rel=\"stylesheet\" href=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + " />" + Environment.NewLine;
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
