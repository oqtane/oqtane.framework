using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;
using System;
using System.Runtime.InteropServices.ComTypes;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IModuleRepository _modules;
        private readonly IUserPermissions _userPermissions;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantResolver _resolver;
        private readonly ITenantRepository _tenants;
        private readonly ISqlRepository _sql;
        private readonly ILogManager _logger;

        public ModuleDefinitionController(IModuleDefinitionRepository moduleDefinitions, IModuleRepository modules, IUserPermissions userPermissions, IInstallationManager installationManager, IWebHostEnvironment environment, ITenantResolver resolver, ITenantRepository tenants, ISqlRepository sql, ILogManager logger)
        {
            _moduleDefinitions = moduleDefinitions;
            _modules = modules;
            _userPermissions = userPermissions;
            _installationManager = installationManager;
            _environment = environment;
            _resolver = resolver;
            _tenants = tenants;
            _sql = sql;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(string siteid)
        {
            List<ModuleDefinition> moduledefinitions = new List<ModuleDefinition>();
            foreach(ModuleDefinition moduledefinition in _moduleDefinitions.GetModuleDefinitions(int.Parse(siteid)))
            {
                if (_userPermissions.IsAuthorized(User,PermissionNames.Utilize, moduledefinition.Permissions))
                {
                    moduledefinitions.Add(moduledefinition);
                }
            }
            return moduledefinitions;
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        public ModuleDefinition Get(int id, string siteid)
        {
            ModuleDefinition moduledefinition = _moduleDefinitions.GetModuleDefinition(id, int.Parse(siteid));
            if (_userPermissions.IsAuthorized(User,PermissionNames.Utilize, moduledefinition.Permissions))
            {
                return moduledefinition;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access ModuleDefinition {ModuleDefinition}", moduledefinition);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int id, [FromBody] ModuleDefinition moduleDefinition)
        {
            if (ModelState.IsValid)
            {
                _moduleDefinitions.UpdateModuleDefinition(moduleDefinition);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Definition Updated {ModuleDefinition}", moduleDefinition);
            }
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallModules()
        {
            _installationManager.InstallPackages("Modules", true);
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Modules Installed");
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id, int siteid)
        {
            List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(siteid).ToList();
            ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionId == id).FirstOrDefault();
            if (moduledefinition != null)
            {
                // server assembly name should follow client naming convention 
                string assemblyname = Utilities.GetAssemblyName(moduledefinition.ModuleDefinitionName).Replace(".Client",".Server");

                string uninstallScript = "";
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyname);
                if (assembly != null)
                {
                    Stream resourceStream = assembly.GetManifestResourceStream(assemblyname + ".Scripts.Uninstall.sql");
                    if (resourceStream != null)
                    {
                        using (var reader = new StreamReader(resourceStream))
                        {
                            uninstallScript = reader.ReadToEnd();
                        }
                    }
                }

                foreach (Tenant tenant in _tenants.GetTenants())
                {
                    // uninstall module database schema
                    if (!string.IsNullOrEmpty(uninstallScript))
                    {
                        _sql.ExecuteScript(tenant, uninstallScript);
                        _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Uninstall Script Executed For {AssemblyName}", assemblyname);
                    }
                    // clean up module schema versions
                    _sql.ExecuteNonQuery(tenant, "DELETE FROM [dbo].[SchemaVersions] WHERE ScriptName LIKE '" + assemblyname + "%'");
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Schema Versions Removed For {AssemblyName}", assemblyname);
                }

                // format root assembly name
                assemblyname = assemblyname.Replace(".Server", "");

                // clean up module static resource folder
                string folder = Path.Combine(_environment.WebRootPath, "Modules\\" + assemblyname);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Static Resources Removed For {AssemblynName}", assemblyname);
                }

                // remove module assembly from /bin
                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(binfolder, assemblyname + "*.*"))
                {
                    System.IO.File.Delete(file);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Assembly Removed {Filename}", file);
                }

                // remove module definition
                _moduleDefinitions.DeleteModuleDefinition(id, siteid);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Definition Deleted {ModuleDefinitionName}", moduledefinition.Name);

                // restart application
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

        // POST api/<controller>?moduleid=x
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public void Post([FromBody] ModuleDefinition moduleDefinition, string moduleid)
        {
            if (ModelState.IsValid)
            {
                string rootPath;
                DirectoryInfo rootFolder = Directory.GetParent(_environment.ContentRootPath);
                string templatePath = Path.Combine(rootFolder.FullName, "Oqtane.Client\\Modules\\Admin\\ModuleCreator\\Templates\\" + moduleDefinition.Template + "\\");

                if (moduleDefinition.Template == "internal")
                {
                    rootPath = rootFolder.FullName + "\\";
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Modules, Oqtane.Client";
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Manager." + moduleDefinition.Name + "Manager, Oqtane.Server";
                }
                else
                {
                    rootPath = rootFolder.Parent.FullName + "\\" + moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Module\\";
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Modules, " + moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Module.Client";                    
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Manager." + moduleDefinition.Name + "Manager, " + moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Module.Server";
                }

                ProcessTemplatesRecursively(new DirectoryInfo(templatePath), rootPath, rootFolder.Name, templatePath, moduleDefinition);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Definition Created {ModuleDefinition}", moduleDefinition);

                Models.Module module = _modules.GetModule(int.Parse(moduleid));
                module.ModuleDefinitionName = moduleDefinition.ModuleDefinitionName;
                _modules.UpdateModule(module);

                _installationManager.RestartApplication();
            }
        }

        private void ProcessTemplatesRecursively(DirectoryInfo current, string rootPath, string rootFolder, string templatePath, ModuleDefinition moduleDefinition)
        {
            // process folder
            string folderPath = rootPath + current.FullName.Replace(templatePath, "");
            folderPath = folderPath.Replace("[Owner]", moduleDefinition.Owner);
            folderPath = folderPath.Replace("[Module]", moduleDefinition.Name);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            FileInfo[] files = current.GetFiles("*.*");
            if (files != null)
            {
                foreach (FileInfo file in files)
                {
                    // process file
                    string filePath = Path.Combine(folderPath, file.Name);
                    filePath = filePath.Replace("[Owner]", moduleDefinition.Owner);
                    filePath = filePath.Replace("[Module]", moduleDefinition.Name);

                    string text = System.IO.File.ReadAllText(file.FullName);
                    text = text.Replace("[Owner]", moduleDefinition.Owner);
                    text = text.Replace("[Module]", moduleDefinition.Name);
                    text = text.Replace("[Description]", moduleDefinition.Description);
                    text = text.Replace("[RootPath]", rootPath);
                    text = text.Replace("[RootFolder]", rootFolder);
                    text = text.Replace("[ServerManagerType]", moduleDefinition.ServerManagerType);
                    text = text.Replace("[Folder]", folderPath);
                    text = text.Replace("[File]", Path.GetFileName(filePath));
                    text = text.Replace("[FrameworkVersion]", Constants.Version);
                    System.IO.File.WriteAllText(filePath, text);

                    if (Path.GetExtension(filePath).ToLower() == ".sql" && !filePath.ToLower().Contains("uninstall"))
                    {
                        // execute installation script in curent tenant
                        _sql.ExecuteScript(_resolver.GetTenant(), text);
                    }
                }

                DirectoryInfo[] folders = current.GetDirectories();

                foreach (DirectoryInfo folder in folders.Reverse())
                {
                    ProcessTemplatesRecursively(folder, rootPath, rootFolder, templatePath, moduleDefinition);
                }
            }
        }
    }
}
