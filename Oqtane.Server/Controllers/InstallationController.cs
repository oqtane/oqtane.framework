using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Oqtane.Themes;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using Oqtane.Repository;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class InstallationController : Controller
    {
        private readonly IConfigManager _configManager;
        private readonly IInstallationManager _installationManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _accessor;
        private readonly IAliasRepository _aliases;
        private readonly ILogger<InstallationController> _filelogger;

        public InstallationController(IConfigManager configManager, IInstallationManager installationManager, IDatabaseManager databaseManager, ILocalizationManager localizationManager, IMemoryCache cache, IHttpContextAccessor accessor, IAliasRepository aliases, ILogger<InstallationController> filelogger)
        {
            _configManager = configManager;
            _installationManager = installationManager;
            _databaseManager = databaseManager;
            _localizationManager = localizationManager;
            _cache = cache;
            _accessor = accessor;
            _aliases = aliases;
            _filelogger = filelogger;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<Installation> Post([FromBody] InstallConfig config)
        {
            var installation = new Installation { Success = false, Message = "" };

            if (ModelState.IsValid && (User.IsInRole(RoleNames.Host) || string.IsNullOrEmpty(_configManager.GetSetting("ConnectionStrings:" + SettingKeys.ConnectionStringKey, ""))))
            {
                installation = _databaseManager.Install(config);

                if (installation.Success && config.Register)
                {
                    await RegisterContact(config.HostEmail);
                }
            }
            else
            {
                installation.Message = "Installation Not Authorized";
            }

            return installation;
        }

        // GET api/<controller>/installed/?path=xxx
        [HttpGet("installed")]
        public Installation IsInstalled(string path)
        {
            var installation = _databaseManager.IsInstalled();
            if (installation.Success)
            {
                path = _accessor.HttpContext.Request.Host.Value + "/" + WebUtility.UrlDecode(path);
                installation.Alias = _aliases.GetAlias(path);
            }
            return installation;
        }

        [HttpGet("upgrade")]
        [Authorize(Roles = RoleNames.Host)]
        public Installation Upgrade()
        {
            var installation = new Installation { Success = true, Message = "" };
            _installationManager.UpgradeFramework();
            return installation;
        }

        // GET api/<controller>/restart
        [HttpPost("restart")]
        [Authorize(Roles = RoleNames.Host)]
        public void Restart()
        {
            _installationManager.RestartApplication();
        }

        // GET api/<controller>/list
        [HttpGet("list")]
        public List<string> List()
        {
            return GetAssemblyList();
        }

        // GET api/<controller>/load?list=x,y
        [HttpGet("load")]
        public IActionResult Load(string list = "*")
        {
            return File(GetAssemblies(list), System.Net.Mime.MediaTypeNames.Application.Octet, "oqtane.dll");
        }

        private List<string> GetAssemblyList()
        {
            // get list of assemblies which should be downloaded to client
            var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var assemblies = AppDomain.CurrentDomain.GetOqtaneClientAssemblies();
            var list = assemblies.Select(a => a.GetName().Name).ToList();

            // include version numbers
            for (int i = 0; i < list.Count; i++)
            {
                 list[i] = Path.GetFileName(AddFileDate(Path.Combine(binFolder, list[i] + ".dll")));
            }

            // insert satellite assemblies at beginning of list
            foreach (var culture in _localizationManager.GetInstalledCultures())
            {
                var assembliesFolderPath = Path.Combine(binFolder, culture);
                if (culture == Constants.DefaultCulture)
                {
                    continue;
                }

                if (Directory.Exists(assembliesFolderPath))
                {
                    foreach (var resourceFile in Directory.EnumerateFiles(assembliesFolderPath))
                    {
                        list.Insert(0, culture + "/" + Path.GetFileName(AddFileDate(resourceFile)));
                    }
                }
                else
                {
                    _filelogger.LogError(Utilities.LogMessage(this, $"The Satellite Assembly Folder For {culture} Does Not Exist"));
                }
            }

            // insert module and theme dependencies at beginning of list
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IModule))))
                {
                    var instance = Activator.CreateInstance(type) as IModule;
                    foreach (string name in instance.ModuleDefinition.Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var path = Path.Combine(binFolder, name + ".dll");
                        if (System.IO.File.Exists(path))
                        {
                            path = Path.GetFileName(AddFileDate(path));
                            if (!list.Contains(path)) list.Insert(0, path);
                        }
                        else
                        {
                            _filelogger.LogError(Utilities.LogMessage(this, $"Module {instance.ModuleDefinition.ModuleDefinitionName} Dependency {name}.dll Does Not Exist"));
                        }
                    }
                }
                foreach (var type in assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ITheme))))
                {
                    var instance = Activator.CreateInstance(type) as ITheme;
                    foreach (string name in instance.Theme.Dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var path = Path.Combine(binFolder, name + ".dll");
                        if (System.IO.File.Exists(path))
                        {
                            path = Path.GetFileName(AddFileDate(path));
                            if (!list.Contains(path)) list.Insert(0, path);
                        }
                        else
                        {
                            _filelogger.LogError(Utilities.LogMessage(this, $"Theme {instance.Theme.ThemeName} Dependency {name}.dll Does Not Exist"));
                        }
                    }
                }
            }

            return list;
        }

        private byte[] GetAssemblies(string list)
        {
            if (list == "*")
            {
                return _cache.GetOrCreate("assemblies", entry =>
                {
                    return GetZIP(list);
                });
            }
            else
            {
                return GetZIP(list);
            }
        }

        private byte[] GetZIP(string list)
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // get list of assemblies which should be downloaded to client
            List<string> assemblies;
            if (list == "*")
            {
                assemblies = GetAssemblyList();
            }
            else
            {
                assemblies = list.Split(',').ToList();
            }

            // create zip file containing assemblies and debug symbols
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string file in assemblies)
                    {
                        var filename = RemoveFileDate(file);
                        if (System.IO.File.Exists(Path.Combine(binFolder, filename)))
                        {
                            using (var filestream = new FileStream(Path.Combine(binFolder, filename), FileMode.Open, FileAccess.Read))
                            using (var entrystream = archive.CreateEntry(file).Open())
                            {
                                filestream.CopyTo(entrystream);
                            }
                        }
                        filename = filename.Replace(".dll", ".pdb");
                        if (System.IO.File.Exists(Path.Combine(binFolder, filename)))
                        {
                            using (var filestream = new FileStream(Path.Combine(binFolder, filename), FileMode.Open, FileAccess.Read))
                            using (var entrystream = archive.CreateEntry(file.Replace(".dll", ".pdb")).Open())
                            {
                                filestream.CopyTo(entrystream);
                            }
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private string AddFileDate(string filepath)
        {
            DateTime lastwritetime = System.IO.File.GetLastWriteTime(filepath);
            return Path.GetFileNameWithoutExtension(filepath) + "." + lastwritetime.ToString("yyyyMMddHHmmss") + Path.GetExtension(filepath);            
        }

        private string RemoveFileDate(string filepath)
        {
            var segments = filepath.Split(".");
            return string.Join(".", segments, 0, segments.Length - 2) + Path.GetExtension(filepath);
        }

        private async Task RegisterContact(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    Uri uri = new Uri(Constants.PackageRegistryUrl + $"/api/registry/contact/?id={_configManager.GetInstallationId()}&email={WebUtility.UrlEncode(email)}");
                    var response = await client.GetAsync(uri).ConfigureAwait(false);
                }
            }
            catch
            {
                // error calling registry service
            }
        }

        // GET api/<controller>/register?email=x
        [HttpPost("register")]
        [Authorize(Roles = RoleNames.Host)]
        public async Task Register(string email)
        {
            await RegisterContact(email);
        }
    }
}
