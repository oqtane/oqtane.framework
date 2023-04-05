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
using System.Text.Json;
using System.Net;
using Oqtane.Modules;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pagemodules;
        private readonly IPermissionRepository _permissions;
        private readonly ITenantRepository _tenants;
        private readonly ISqlRepository _sql;
        private readonly IUserPermissions _userPermissions;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantManager _tenantManager;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ModuleDefinitionController(IModuleDefinitionRepository moduleDefinitions, IModuleRepository module, IPageModuleRepository pageModule, IPermissionRepository permission, ITenantRepository tenants, ISqlRepository sql, IUserPermissions userPermissions, IInstallationManager installationManager, IWebHostEnvironment environment, IServiceProvider serviceProvider, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _moduleDefinitions = moduleDefinitions;
            _modules = module;
            _pagemodules = pageModule;
            _permissions = permission;
            _tenants = tenants;
            _sql = sql;
            _userPermissions = userPermissions;
            _installationManager = installationManager;
            _environment = environment;
            _serviceProvider = serviceProvider;
            _tenantManager = tenantManager;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                List<ModuleDefinition> moduledefinitions = new List<ModuleDefinition>();
                foreach (ModuleDefinition moduledefinition in _moduleDefinitions.GetModuleDefinitions(SiteId))
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.Utilize, moduledefinition.PermissionList))
                    {
                        if (string.IsNullOrEmpty(moduledefinition.Version)) moduledefinition.Version = new Version(1, 0, 0).ToString();
                        moduledefinitions.Add(moduledefinition);
                    }
                }
                return moduledefinitions;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        public ModuleDefinition Get(int id, string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                ModuleDefinition moduledefinition = _moduleDefinitions.GetModuleDefinition(id, SiteId);
                if (_userPermissions.IsAuthorized(User, PermissionNames.Utilize, moduledefinition.PermissionList))
                {
                    if (string.IsNullOrEmpty(moduledefinition.Version)) moduledefinition.Version = new Version(1, 0, 0).ToString();
                    return moduledefinition;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Get Attempt {ModuleDefinitionId} {SiteId}", id, siteid);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Get Attempt {ModuleDefinitionId} {SiteId}", id, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public ModuleDefinition Post([FromBody] ModuleDefinition moduleDefinition)
        {
            if (ModelState.IsValid)
            {
                string rootPath;
                DirectoryInfo rootFolder = Directory.GetParent(_environment.ContentRootPath);
                string templatePath = Utilities.PathCombine(_environment.WebRootPath, "Modules", "Templates", moduleDefinition.Template, Path.DirectorySeparatorChar.ToString());

                if (moduleDefinition.Template.ToLower().Contains("internal"))
                {
                    rootPath = Utilities.PathCombine(rootFolder.FullName, Path.DirectorySeparatorChar.ToString());
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + ", Oqtane.Client";
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + ".Manager." + moduleDefinition.Name + "Manager, Oqtane.Server";
                }
                else
                {
                    rootPath = Utilities.PathCombine(rootFolder.Parent.FullName, moduleDefinition.Owner + "." + moduleDefinition.Name, Path.DirectorySeparatorChar.ToString());
                    moduleDefinition.ModuleDefinitionName = moduleDefinition.Owner + "." + moduleDefinition.Name + ", " + moduleDefinition.Owner + "." + moduleDefinition.Name + ".Client.Oqtane";
                    moduleDefinition.ServerManagerType = moduleDefinition.Owner + "." + moduleDefinition.Name + ".Manager." + moduleDefinition.Name + "Manager, " + moduleDefinition.Owner + "." + moduleDefinition.Name + ".Server.Oqtane";
                }

                ProcessTemplatesRecursively(new DirectoryInfo(templatePath), rootPath, rootFolder.Name, templatePath, moduleDefinition);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Definition Created {ModuleDefinition}", moduleDefinition);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Post Attempt {ModuleDefinition}", moduleDefinition);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                moduleDefinition = null;
            }

            return moduleDefinition;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Put(int id, [FromBody] ModuleDefinition moduleDefinition)
        {
            if (ModelState.IsValid && moduleDefinition.SiteId == _alias.SiteId && _moduleDefinitions.GetModuleDefinition(moduleDefinition.ModuleDefinitionId, moduleDefinition.SiteId) != null)
            {
                _moduleDefinitions.UpdateModuleDefinition(moduleDefinition);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.ModuleDefinition, moduleDefinition.ModuleDefinitionId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Definition Updated {ModuleDefinition}", moduleDefinition);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Put Attempt {ModuleDefinition}", moduleDefinition);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id, int siteid)
        {
            ModuleDefinition moduledefinition = _moduleDefinitions.GetModuleDefinition(id, siteid);
            if (moduledefinition != null && moduledefinition.SiteId == _alias.SiteId && Utilities.GetAssemblyName(moduledefinition.ServerManagerType) != "Oqtane.Server")
            {
                // execute uninstall logic or scripts
                if (!string.IsNullOrEmpty(moduledefinition.ServerManagerType))
                {
                    Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                    if (moduletype != null)
                    {
                        var alias = _tenantManager.GetAlias(); // save current
                        string result = string.Empty;
                        foreach (Tenant tenant in _tenants.GetTenants())
                        {
                            try
                            {
                                if (moduletype.GetInterface("IInstallable") != null)
                                {
                                    _tenantManager.SetTenant(tenant.TenantId);
                                    var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                    ((IInstallable)moduleobject).Uninstall(tenant);
                                }
                                else
                                {
                                    _sql.ExecuteScript(tenant, moduletype.Assembly, Utilities.GetTypeName(moduledefinition.ModuleDefinitionName) + ".Uninstall.sql");
                                }
                            }
                            catch (Exception ex)
                            {
                                result = "For " + tenant.Name + " " + ex.Message;
                            }
                        }
                        _tenantManager.SetAlias(alias); // restore current
                        if (string.IsNullOrEmpty(result))
                        {
                            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "{ModuleDefinitionName} Uninstalled For All Tenants", moduledefinition.ModuleDefinitionName);
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Error Uninstalling {ModuleDefinitionName} {Error}", moduledefinition.ModuleDefinitionName, result);
                        }
                    }
                }

                // remove module assets
                if (_installationManager.UninstallPackage(moduledefinition.PackageName))
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Assets Removed For {ModuleDefinitionName}", moduledefinition.ModuleDefinitionName);
                }
                else
                {
                    // attempt to delete assemblies based on naming convention
                    foreach(string asset in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Utilities.GetTypeName(moduledefinition.ModuleDefinitionName) + "*.*"))
                    {
                        System.IO.File.Delete(asset);
                    }
                    _logger.Log(LogLevel.Warning, this, LogFunction.Delete, "Module Assets Removed For {ModuleDefinitionName}. Please Note That Some Assets May Have Been Missed Due To A Missing Asset Manifest. An Asset Manifest Is Only Created If A Module Is Installed From A Nuget Package.", moduledefinition.Name);
                }

                // clean up module static resource folder
                string assetpath = Path.Combine(_environment.WebRootPath, "Modules", Utilities.GetTypeName(moduledefinition.ModuleDefinitionName));
                if (Directory.Exists(assetpath))
                {
                    Directory.Delete(assetpath, true);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Static Resources Folder Removed For {ModuleDefinitionName}", moduledefinition.ModuleDefinitionName);
                }

                // remove PageModule and Module
                List<Models.Module> modulesToRemove =  _modules.GetModules(moduledefinition.SiteId).Where(m => m.ModuleDefinitionName == moduledefinition.ModuleDefinitionName).ToList();
                foreach (Models.Module moduleToRemove in modulesToRemove)
                {
                    // Get the PageModule items associated with the Module item to be removed
                    List<PageModule> pageModulesToRemove = _pagemodules.GetPageModules(moduledefinition.SiteId).Where(pm => pm.ModuleId == moduleToRemove.ModuleId).ToList();

                    foreach(PageModule pageModule in pageModulesToRemove)
                    {
                        // Remove the PageModule item
                        _pagemodules.DeletePageModule(pageModule.PageModuleId);
                    }

                    // Remove Permissions
                    _permissions.DeletePermissions(moduledefinition.SiteId, EntityNames.Module, moduleToRemove.ModuleId);

                    // Remove the Module item
                    _modules.DeleteModule(moduleToRemove.ModuleId);
                }


                // remove module definition
                _moduleDefinitions.DeleteModuleDefinition(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.ModuleDefinition, moduledefinition.ModuleDefinitionId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Definition {ModuleDefinitionName} Deleted", moduledefinition.Name);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ModuleDefinition Delete Attempt {ModuleDefinitionId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET: api/<controller>/templates
        [HttpGet("templates")]
        [Authorize(Roles = RoleNames.Host)]
        public List<Template> GetTemplates()
        {
            var templates = new List<Template>();
            var root = Directory.GetParent(_environment.ContentRootPath);
            string templatePath = Utilities.PathCombine(_environment.WebRootPath, "Modules", "Templates", Path.DirectorySeparatorChar.ToString());
            foreach (string directory in Directory.GetDirectories(templatePath))
            {
                string name = directory.Replace(templatePath, "");
                if (System.IO.File.Exists(Path.Combine(directory, "template.json")))
                {
                    var template = JsonSerializer.Deserialize<Template>(System.IO.File.ReadAllText(Path.Combine(directory, "template.json")));
                    template.Name = name;
                    template.Location = "";
                    if (template.Type.ToLower() != "internal")
                    {
                        template.Location = Utilities.PathCombine(root.Parent.ToString(), Path.DirectorySeparatorChar.ToString());
                    }
                    templates.Add(template);
                }
                else
                {
                    templates.Add(new Template { Name = name, Title = name, Type = "External", Version = "", Location = Utilities.PathCombine(root.Parent.ToString(), Path.DirectorySeparatorChar.ToString()) });
                }
            }
            return templates;
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
                    if (moduleDefinition.Version == "local")
                    {
                        text = text.Replace("[FrameworkVersion]", Constants.Version);
                        text = text.Replace("[ClientReference]", $"<Reference Include=\"Oqtane.Client\"><HintPath>..\\..\\{rootFolder}\\Oqtane.Server\\bin\\Debug\\net6.0\\Oqtane.Client.dll</HintPath></Reference>");
                        text = text.Replace("[ServerReference]", $"<Reference Include=\"Oqtane.Server\"><HintPath>..\\..\\{rootFolder}\\Oqtane.Server\\bin\\Debug\\net6.0\\Oqtane.Server.dll</HintPath></Reference>");
                        text = text.Replace("[SharedReference]", $"<Reference Include=\"Oqtane.Shared\"><HintPath>..\\..\\{rootFolder}\\Oqtane.Server\\bin\\Debug\\net6.0\\Oqtane.Shared.dll</HintPath></Reference>");
                    }
                    else
                    {
                        text = text.Replace("[FrameworkVersion]", moduleDefinition.Version);
                        text = text.Replace("[ClientReference]", "<PackageReference Include=\"Oqtane.Client\" Version=\"" + moduleDefinition.Version + "\" />");
                        text = text.Replace("[ServerReference]", "<PackageReference Include=\"Oqtane.Server\" Version=\"" + moduleDefinition.Version + "\" />");
                        text = text.Replace("[SharedReference]", "<PackageReference Include=\"Oqtane.Shared\" Version=\"" + moduleDefinition.Version + "\" />");
                    }
                    System.IO.File.WriteAllText(filePath, text);
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
