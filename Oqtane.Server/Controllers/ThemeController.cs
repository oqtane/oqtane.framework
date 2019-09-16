using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Oqtane.Shared;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using System.IO.Compression;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository Themes;
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly IWebHostEnvironment environment;

        public ThemeController(IThemeRepository Themes, IHostApplicationLifetime HostApplicationLifetime, IWebHostEnvironment environment)
        {
            this.Themes = Themes;
            this.HostApplicationLifetime = HostApplicationLifetime;
            this.environment = environment;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Theme> Get()
        {
            return Themes.GetThemes();
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallThemes()
        {
            bool install = false;
            string themefolder = Path.Combine(environment.WebRootPath, "Themes");
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // iterate through theme packages
            foreach (string packagename in Directory.GetFiles(themefolder, "*.nupkg"))
            {
                // iterate through files and deploy to appropriate locations
                using (ZipArchive archive = ZipFile.OpenRead(packagename))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string filename = Path.GetFileName(entry.FullName);
                        switch (Path.GetExtension(filename))
                        {
                            case ".dll":
                                entry.ExtractToFile(Path.Combine(binfolder, filename));
                                break;
                        }
                    }
                }
                // remove theme package
                System.IO.File.Delete(packagename);
                install = true;
            }

            if (install)
            {
                // restart application
                HostApplicationLifetime.StopApplication();
            }
        }
    }
}
