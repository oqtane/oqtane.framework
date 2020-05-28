using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository _themes;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogManager _logger;

        public ThemeController(IThemeRepository themes, IInstallationManager installationManager, IWebHostEnvironment environment, ILogManager logger)
        {
            _themes = themes;
            _installationManager = installationManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<Theme> Get()
        {
            return _themes.GetThemes();
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallThemes()
        {
            _installationManager.InstallPackages("Themes", true);
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Themes Installed");
        }

        // DELETE api/<controller>/xxx
        [HttpDelete("{themename}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(string themename)
        {
            List<Theme> themes = _themes.GetThemes().ToList();
            Theme theme = themes.Where(item => item.ThemeName == themename).FirstOrDefault();
            if (theme != null && Utilities.GetAssemblyName(theme.ThemeName) != "Oqtane.Client")
            {
                // clean up theme static resource folder
                string folder = Path.Combine(_environment.WebRootPath, "Themes" , Utilities.GetTypeName(theme.ThemeName));
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Static Resources Removed For {ThemeName}", theme.ThemeName);
                }

                // remove theme assembly from /bin
                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                System.IO.File.Delete(Path.Combine(binfolder, Utilities.GetAssemblyName(theme.ThemeName) + ".dll"));
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Assembly {Filename} Removed For {ThemeName}", Utilities.GetAssemblyName(theme.ThemeName) + ".dll", themename);

                _installationManager.RestartApplication();
            }
        }

        // GET api/<controller>/load/assembyname
        [HttpGet("load/{assemblyname}")]
        public IActionResult Load(string assemblyname)
        {
            if (Path.GetExtension(assemblyname).ToLower() == ".dll")
            {
                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binfolder, assemblyname));
                return File(file, "application/octet-stream", assemblyname);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Download Assembly {Assembly}", assemblyname);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

    }
}
