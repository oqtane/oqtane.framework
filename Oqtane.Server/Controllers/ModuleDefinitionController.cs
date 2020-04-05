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
        private readonly ISqlRepository _sql;
        private readonly ILogManager _logger;

        public ModuleDefinitionController(IModuleDefinitionRepository moduleDefinitions, IModuleRepository modules, IUserPermissions userPermissions, IInstallationManager installationManager, IWebHostEnvironment environment, ITenantResolver resolver, ISqlRepository sql, ILogManager logger)
        {
            _moduleDefinitions = moduleDefinitions;
            _modules = modules;
            _userPermissions = userPermissions;
            _installationManager = installationManager;
            _environment = environment;
            _resolver = resolver;
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
                string moduledefinitionname = moduledefinition.ModuleDefinitionName.Substring(0, moduledefinition.ModuleDefinitionName.IndexOf(","));

                string folder = Path.Combine(_environment.WebRootPath, "Modules\\" + moduledefinitionname);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }

                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(binfolder, moduledefinitionname + "*.dll"))
                {
                    System.IO.File.Delete(file);
                }

                _moduleDefinitions.DeleteModuleDefinition(id, siteid);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleDefinitionId}", id);

                _installationManager.RestartApplication();
            }
        }

        // GET api/<controller>/load/filename
        [HttpGet("load/{filename}")]
        public IActionResult Load(string assemblyname)
        {
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binfolder, assemblyname));
            return File(file, "application/octet-stream", assemblyname);
        }

        // POST api/<controller>?moduleid=x
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public void Post([FromBody] ModuleDefinition moduleDefinition, string moduleid)
        {
            if (ModelState.IsValid)
            {
                string rootPath = Directory.GetParent(_environment.ContentRootPath).FullName;
                string templatePath = Path.Combine(rootPath, "Oqtane.Client\\Modules\\Admin\\ModuleCreator\\Templates\\");
                ProcessTemplatesRecursively(new DirectoryInfo(templatePath), rootPath, moduleDefinition);
                moduleDefinition.ModuleDefinitionName = "Oqtane.Modules." + moduleDefinition.Name + "s, Oqtane.Client";
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Definition Created {ModuleDefinition}", moduleDefinition);
                Models.Module module = _modules.GetModule(int.Parse(moduleid));
                module.ModuleDefinitionName = moduleDefinition.ModuleDefinitionName;
                _modules.UpdateModule(module);
                _installationManager.RestartApplication();
            }
        }

        private void ProcessTemplatesRecursively(DirectoryInfo current, string rootPath, ModuleDefinition moduleDefinition)
        {
            // process folder
            string folderPath = current.FullName.Replace("Oqtane.Client\\Modules\\Admin\\ModuleCreator\\Templates\\", "");
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
                    filePath = filePath.Replace("[Module]", moduleDefinition.Name);

                    string text = System.IO.File.ReadAllText(file.FullName);
                    text = text.Replace("[Module]", moduleDefinition.Name);
                    text = text.Replace("[Description]", moduleDefinition.Description);
                    text = text.Replace("[RootPath]", rootPath);
                    text = text.Replace("[Folder]", folderPath);
                    text = text.Replace("[File]", Path.GetFileName(filePath));
                    System.IO.File.WriteAllText(filePath, text);

                    if (Path.GetExtension(filePath) == ".sql")
                    {
                        // execute script
                        foreach (string query in text.Split("GO", StringSplitOptions.RemoveEmptyEntries))
                        {
                            _sql.ExecuteNonQuery(_resolver.GetTenant(), query);
                        }
                    }
                }

                DirectoryInfo[] folders = current.GetDirectories();

                foreach (DirectoryInfo folder in folders.Reverse())
                {
                    ProcessTemplatesRecursively(folder, rootPath, moduleDefinition);
                }
            }
        }
    }
}
