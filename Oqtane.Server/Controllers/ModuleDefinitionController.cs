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

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IInstallationManager InstallationManager;
        private readonly IWebHostEnvironment environment;
        private readonly ILogManager logger;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions, IInstallationManager InstallationManager, IWebHostEnvironment environment, ILogManager logger)
        {
            this.ModuleDefinitions = ModuleDefinitions;
            this.InstallationManager = InstallationManager;
            this.environment = environment;
            this.logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(int siteid)
        {
            return ModuleDefinitions.GetModuleDefinitions(siteid);
        }

        // GET api/<controller>/filename
        [HttpGet("{filename}")]
        public IActionResult Get(string assemblyname)
        {
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binfolder, assemblyname));
            return File(file, "application/octet-stream", assemblyname);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int id, [FromBody] ModuleDefinition ModuleDefinition)
        {
            if (ModelState.IsValid)
            {
                ModuleDefinitions.UpdateModuleDefinition(ModuleDefinition);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Definition Updated {ModuleDefinition}", ModuleDefinition);
            }
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallModules()
        {
            InstallationManager.InstallPackages("Modules", true);
            logger.Log(LogLevel.Information, this, LogFunction.Create, "Modules Installed");
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id, int siteid)
        {
            List<ModuleDefinition> moduledefinitions = ModuleDefinitions.GetModuleDefinitions(siteid).ToList();
            ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionId == id).FirstOrDefault();
            if (moduledefinition != null)
            {
                string moduledefinitionname = moduledefinition.ModuleDefinitionName.Substring(0, moduledefinition.ModuleDefinitionName.IndexOf(","));

                string folder = Path.Combine(environment.WebRootPath, "Modules\\" + moduledefinitionname);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }

                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(binfolder, moduledefinitionname + "*.dll"))
                {
                    System.IO.File.Delete(file);
                }

                ModuleDefinitions.DeleteModuleDefinition(id, siteid);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleDefinitionId}", id);

                InstallationManager.RestartApplication();
            }
        }
    }
}
