using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository _themes;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogManager _logger;
        private readonly IStringLocalizer _localizer;

        public ThemeController(IThemeRepository themes, IInstallationManager installationManager, IWebHostEnvironment environment, ILogManager logger, IStringLocalizer<ThemeController> localizer)
        {
            _themes = themes;
            _installationManager = installationManager;
            _environment = environment;
            _logger = logger;
            _localizer = localizer;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Theme> Get()
        {
            return _themes.GetThemes();
        }

        [HttpGet("install")]
        [Authorize(Roles = RoleNames.Host)]
        public void InstallThemes()
        {
            _logger.Log(LogLevel.Information, this, LogFunction.Create, _localizer["Themes Installed"]);
            _installationManager.InstallPackages("Themes");
        }

        // DELETE api/<controller>/xxx
        [HttpDelete("{themename}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(string themename)
        {
            List<Theme> themes = _themes.GetThemes().ToList();
            Theme theme = themes.Where(item => item.ThemeName == themename).FirstOrDefault();
            if (theme != null && Utilities.GetAssemblyName(theme.ThemeName) != "Oqtane.Client")
            {
                // use assets.json to clean up file resources
                string assetfilepath = Path.Combine(_environment.WebRootPath, "Themes", Utilities.GetTypeName(theme.ThemeName), "assets.json");
                if (System.IO.File.Exists(assetfilepath))
                {
                    List<string> assets = JsonSerializer.Deserialize<List<string>>(System.IO.File.ReadAllText(assetfilepath));
                    foreach (string asset in assets)
                    {
                        if (System.IO.File.Exists(asset))
                        {
                            System.IO.File.Delete(asset);
                        }
                    }
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Theme Assets Removed For {ThemeName}"], theme.ThemeName);
                }

                // clean up theme static resource folder
                string folder = Path.Combine(_environment.WebRootPath, "Themes" , Utilities.GetTypeName(theme.ThemeName));
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Theme Resource Folder Removed For {ThemeName}"], theme.ThemeName);
                }
            }
        }

    }
}
