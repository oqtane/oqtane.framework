using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IUserPermissions _userPermissions;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogManager _logger;

        public ModuleDefinitionController(IModuleDefinitionRepository moduleDefinitions, IUserPermissions userPermissions, IInstallationManager installationManager, IWebHostEnvironment environment, ILogManager logger)
        {
            _moduleDefinitions = moduleDefinitions;
            _userPermissions = userPermissions;
            _installationManager = installationManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(string siteId)
        {
            var user = _userPermissions.GetUser(User);
            var isHost = User.IsInRole(Constants.HostRole);
            if (int.TryParse(siteId, out int id))
            {
                var moduleDefinitions = _moduleDefinitions.GetModuleDefinitions(id)
                    .Where(m => isHost || UserSecurity.IsAuthorized(user, "Utilize", m.Permissions))
                    .ToList();
                return moduleDefinitions;
            }

            return new List<ModuleDefinition>();
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        public ModuleDefinition Get(int id, string siteId)
        {
            var isHost = User.IsInRole(Constants.HostRole);
            ModuleDefinition moduleDefinition = _moduleDefinitions.GetModuleDefinition(id, int.Parse(siteId));
            if (isHost || _userPermissions.IsAuthorized(User, "Utilize", moduleDefinition.Permissions))
            {
                return moduleDefinition;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read,
                    "User Not Authorized To Access ModuleDefinition {ModuleDefinition}", moduleDefinition);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int id, [FromBody] ModuleDefinition ModuleDefinition)
        {
            if (ModelState.IsValid)
            {
                _moduleDefinitions.UpdateModuleDefinition(ModuleDefinition);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Definition Updated {ModuleDefinition}", ModuleDefinition);
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
        public void Delete(int id, int siteId)
        {
            List<ModuleDefinition> moduleDefinitions = _moduleDefinitions.GetModuleDefinitions(siteId).ToList();
            ModuleDefinition moduleDefinition = moduleDefinitions.FirstOrDefault(item => item.ModuleDefinitionId == id);
            if (moduleDefinition != null)
            {
                string moduleDefinitionName = moduleDefinition.ModuleDefinitionName.Substring(0,moduleDefinition.ModuleDefinitionName.IndexOf(","));

                string folder = Path.Combine(_environment.WebRootPath, "Modules\\" + moduleDefinitionName);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }

                string binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(binFolder, moduleDefinitionName + "*.dll"))
                {
                    System.IO.File.Delete(file);
                }

                _moduleDefinitions.DeleteModuleDefinition(id, siteId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleDefinitionId}", id);

                _installationManager.RestartApplication();
            }
        }

        // GET api/<controller>/load/filename
        [HttpGet("load/{filename}")]
        public IActionResult Load(string assemblyName)
        {
            string binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binFolder, assemblyName));
            return File(file, "application/octet-stream", assemblyName);
        }
    }
}
