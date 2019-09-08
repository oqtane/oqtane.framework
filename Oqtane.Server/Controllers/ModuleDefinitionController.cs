using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Reflection;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly IWebHostEnvironment environment;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions, IHostApplicationLifetime HostApplicationLifetime, IWebHostEnvironment environment)
        {
            this.ModuleDefinitions = ModuleDefinitions;
            this.HostApplicationLifetime = HostApplicationLifetime;
            this.environment = environment;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get()
        {
            return ModuleDefinitions.GetModuleDefinitions();
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallModules()
        {
            bool install = false;
            string modulefolder = Path.Combine(environment.WebRootPath, "Modules");
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // iterate through module packages
            foreach (string packagename in Directory.GetFiles(modulefolder, "*.nupkg"))
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
                // remove module package
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
