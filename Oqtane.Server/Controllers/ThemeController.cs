using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository Themes;
        private readonly IInstallationManager InstallationManager;
        private readonly IWebHostEnvironment environment;
        private readonly ILogManager logger;

        public ThemeController(IThemeRepository Themes, IInstallationManager InstallationManager, IWebHostEnvironment environment, ILogManager logger)
        {
            this.Themes = Themes;
            this.InstallationManager = InstallationManager;
            this.environment = environment;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Theme> Get()
        {
            return Themes.GetThemes();
        }

        // GET api/<controller>/filename
        [HttpGet("{filename}")]
        public IActionResult Get(string filename)
        {
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binfolder, filename));
            return File(file, "application/octet-stream", filename);
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallThemes()
        {
            InstallationManager.InstallPackages("Themes", true);
            logger.Log(LogLevel.Information, this, LogFunction.Create, "Themes Installed");
        }

        // DELETE api/<controller>/xxx
        [HttpDelete("{themename}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(string themename)
        {
            List<Theme> themes = Themes.GetThemes().ToList();
            Theme theme = themes.Where(item => item.ThemeName == themename).FirstOrDefault();
            if (theme != null)
            {
                themename = theme.ThemeName.Substring(0, theme.ThemeName.IndexOf(","));

                string folder = Path.Combine(environment.WebRootPath, "Themes\\" + themename);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }

                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(binfolder, themename + "*.dll"))
                {
                    System.IO.File.Delete(file);
                }
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Deleted {ThemeName}", themename);

                InstallationManager.RestartApplication();
            }
        }
    }
}
