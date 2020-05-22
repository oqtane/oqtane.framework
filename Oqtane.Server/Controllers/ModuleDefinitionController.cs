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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IModuleRepository _modules;
        private readonly ITenantRepository _tenants;
        private readonly ISqlRepository _sql;
        private readonly IUserPermissions _userPermissions;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogManager _logger;

        public ModuleDefinitionController(IModuleDefinitionRepository moduleDefinitions, IModuleRepository modules,ITenantRepository tenants, ISqlRepository sql, IUserPermissions userPermissions, IInstallationManager installationManager, IWebHostEnvironment environment, IConfigurationRoot config, IServiceProvider serviceProvider, ILogManager logger)
        {
            _moduleDefinitions = moduleDefinitions;
            _modules = modules;
            _tenants = tenants;
            _sql = sql;
            _userPermissions = userPermissions;
            _installationManager = installationManager;
            _environment = environment;
            _config = config;
            _serviceProvider = serviceProvider;
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
            ModuleDefinition moduledefinition = _moduleDefinitions.GetModuleDefinition(id, siteid);
            if (moduledefinition != null )
            {
                if (!string.IsNullOrEmpty(moduledefinition.ServerManagerType) && Utilities.GetAssemblyName(moduledefinition.ServerManagerType) != "Oqtane.Server")
                {
                    Type moduletype = Type.GetType(moduledefinition.ServerManagerType);

                    foreach (Tenant tenant in _tenants.GetTenants())
                    {
                        try
                        {
                            if (moduletype.GetInterface("IInstallable") != null)
                            {
                                var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                ((IInstallable)moduleobject).Uninstall(tenant);
                            }
                            else
                            {
                                _sql.ExecuteScript(tenant, moduletype.Assembly, Utilities.GetTypeName(moduledefinition.ModuleDefinitionName) + ".Uninstall.sql");
                            }
                            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "{ModuleDefinitionName} Uninstalled For Tenant {Tenant}", moduledefinition.ModuleDefinitionName, tenant.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Error Uninstalling {ModuleDefinitionName} For Tenant {Tenant} {Error}", moduledefinition.ModuleDefinitionName, tenant.Name, ex.Message);
                        }
                    }
                    
                    // clean up module static resource folder
                    string folder = Path.Combine(_environment.WebRootPath, Path.Combine("Modules", Utilities.GetTypeName(moduledefinition.ModuleDefinitionName)));
                    if (Directory.Exists(folder))
                    {
                        Directory.Delete(folder, true);
                        _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Static Resources Removed For {ModuleDefinitionName}", moduledefinition.ModuleDefinitionName);
                    }

                    // get root assembly name ( note that this only works if modules follow a specific naming convention for their assemblies )
                    string assemblyname = Utilities.GetAssemblyName(moduledefinition.ModuleDefinitionName).ToLower();
                    assemblyname = assemblyname.Replace(".client", "").Replace(".oqtane", "");

                    // remove module assemblies from /bin
                    string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    foreach (string file in Directory.EnumerateFiles(binfolder, assemblyname + "*.*"))
                    {
                        System.IO.File.Delete(file);
                        _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Assembly {Filename} Removed For {ModuleDefinitionName}", file, moduledefinition.ModuleDefinitionName);
                    }

                    // remove module definition
                    _moduleDefinitions.DeleteModuleDefinition(id, siteid);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Definition {ModuleDefinitionName} Deleted", moduledefinition.Name);

                    // restart application
                    _installationManager.RestartApplication();
                }
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
                string templatePath = Utilities.PathCombine(_environment.WebRootPath, "Modules", "Templates", moduleDefinition.Template,"\\");

                if (moduleDefinition.Template == "internal")
                {
                    rootPath = Utilities.PathCombine(rootFolder.FullName,"\\");
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + "s, Oqtane.Client";
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Manager." + moduleDefinition.Name + "Manager, Oqtane.Server";
                }
                else
                {
                    rootPath = Utilities.PathCombine(rootFolder.Parent.FullName , moduleDefinition.Owner + "." + moduleDefinition.Name + "s","\\");
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + "s, " + moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Client.Oqtane";                    
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Manager." + moduleDefinition.Name + "Manager, " + moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Server.Oqtane";
                }

                ProcessTemplatesRecursively(new DirectoryInfo(templatePath), rootPath, rootFolder.Name, templatePath, moduleDefinition);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Definition Created {ModuleDefinition}", moduleDefinition);

                Models.Module module = _modules.GetModule(int.Parse(moduleid));
                module.ModuleDefinitionName = moduleDefinition.ModuleDefinitionName;
                _modules.UpdateModule(module);

                if (moduleDefinition.Template == "internal")
                {
                    // add embedded resources to project
                    List<string> resources = new List<string>();
                    resources.Add(Utilities.PathCombine("Modules", moduleDefinition.Owner + "." + moduleDefinition.Name + "s", "Scripts", moduleDefinition.Owner + "." + moduleDefinition.Name + "s.1.0.0.sql"));
                    resources.Add(Utilities.PathCombine("Modules", moduleDefinition.Owner + "." + moduleDefinition.Name + "s", "Scripts", moduleDefinition.Owner + "." + moduleDefinition.Name + "s.Uninstall.sql"));
                    EmbedResourceFiles(Utilities.PathCombine(rootPath, "Oqtane.Server", "Oqtane.Server.csproj"), resources);
                }

                _installationManager.RestartApplication();
            }
        }

        private void ProcessTemplatesRecursively(DirectoryInfo current, string rootPath, string rootFolder, string templatePath, ModuleDefinition moduleDefinition)
        {
            // process folder
            string folderPath = Utilities.PathCombine(rootPath, current.FullName.Replace(templatePath, ""));
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
                }

                DirectoryInfo[] folders = current.GetDirectories();

                foreach (DirectoryInfo folder in folders.Reverse())
                {
                    ProcessTemplatesRecursively(folder, rootPath, rootFolder, templatePath, moduleDefinition);
                }
            }
        }

        private void EmbedResourceFiles(string projectfile, List<string> resources)
        {
            XDocument project = XDocument.Load(projectfile);
            var itemGroup = project.Descendants("ItemGroup").Descendants("EmbeddedResource").FirstOrDefault().Parent;
            if (itemGroup != null)
            {
                foreach (var resource in resources)
                {
                    itemGroup.Add(new XElement("EmbeddedResource", new XAttribute("Include", resource)));
                }
            }
            project.Save(projectfile);
        }
    }
}
