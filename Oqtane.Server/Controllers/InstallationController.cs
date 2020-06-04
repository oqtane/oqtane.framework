using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.IO.Compression;
using Oqtane.Modules;
using Oqtane.Themes;
using System.Diagnostics;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly IInstallationManager _installationManager;
        private readonly IDatabaseManager _databaseManager;

        public InstallationController(IConfigurationRoot config, IInstallationManager installationManager, IDatabaseManager databaseManager)
        {
            _config = config;
            _installationManager = installationManager;
            _databaseManager = databaseManager;
        }

        // POST api/<controller>
        [HttpPost]
        public Installation Post([FromBody] InstallConfig config)
        {
            var installation = new Installation {Success = false, Message = ""};

            if (ModelState.IsValid && (User.IsInRole(Constants.HostRole) || string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.ConnectionStringKey))))
            {
                installation = _databaseManager.Install(config);
            }
            else
            {
                installation.Message = "Installation Not Authorized";
            }

            return installation;
        }

        // GET api/<controller>/installed
        [HttpGet("installed")]
        public Installation IsInstalled()
        {
            bool isInstalled = _databaseManager.IsInstalled();
            return new Installation {Success = isInstalled, Message = string.Empty};
        }

        [HttpGet("upgrade")]
        [Authorize(Roles = Constants.HostRole)]
        public Installation Upgrade()
        {
            var installation = new Installation {Success = true, Message = ""};
            _installationManager.UpgradeFramework();
            return installation;
        }

        // GET api/<controller>/load
        [HttpGet("load")]
        public IActionResult Load()
        {
            if (_config.GetSection("Runtime").Value == "WebAssembly")
            {
                // get list of assemblies which should be downloaded to browser
                var assemblies = AppDomain.CurrentDomain.GetOqtaneClientAssemblies();
                var list = assemblies.Select(a => a.GetName().Name).ToList();

                // get module and theme dependencies
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IModule))))
                    {
                        var instance = Activator.CreateInstance(type) as IModule;
                        foreach (string name in instance.ModuleDefinition.Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!list.Contains(name)) list.Add(name);
                        }
                    }
                    foreach (var type in assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ITheme))))
                    {
                        var instance = Activator.CreateInstance(type) as ITheme;
                        foreach (string name in instance.Theme.Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!list.Contains(name)) list.Add(name);
                        }
                    }
                }

                // create zip file containing assemblies and debug symbols
                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                byte[] zipfile;
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        ZipArchiveEntry entry;
                        foreach (string file in list)
                        {
                            entry = archive.CreateEntry(file + ".dll");
                            using (var filestream = new FileStream(Path.Combine(binfolder, file + ".dll"), FileMode.Open, FileAccess.Read))
                            using (var entrystream = entry.Open())
                            {
                                filestream.CopyTo(entrystream);
                            }

                            // include debug symbols ( we may want to consider restricting this to only host users or when running in debug mode for performance )
                            if (System.IO.File.Exists(Path.Combine(binfolder, file + ".pdb")))
                            {
                                entry = archive.CreateEntry(file + ".pdb");
                                using (var filestream = new FileStream(Path.Combine(binfolder, file + ".pdb"), FileMode.Open, FileAccess.Read))
                                using (var entrystream = entry.Open())
                                {
                                    filestream.CopyTo(entrystream);
                                }
                            }
                        }
                    }
                    zipfile = memoryStream.ToArray();
                }
                return File(zipfile, "application/octet-stream", "oqtane.zip");
            }
            else
            {
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }
    }
}
