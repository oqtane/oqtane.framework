using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;
using System.IO;
using System.Reflection;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IInstallationManager InstallationManager;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions, IInstallationManager InstallationManager)
        {
            this.ModuleDefinitions = ModuleDefinitions;
            this.InstallationManager = InstallationManager;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(int siteid)
        {
            return ModuleDefinitions.GetModuleDefinitions(siteid);
        }

        // GET api/<controller>/filename
        [HttpGet("{filename}")]
        public IActionResult Get(string filename)
        {
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(binfolder, filename));
            return File(file, "application/octet-stream", filename);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int id, [FromBody] ModuleDefinition ModuleDefinition)
        {
            if (ModelState.IsValid)
            {
                ModuleDefinitions.UpdateModuleDefinition(ModuleDefinition);
            }
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallModules()
        {
            InstallationManager.InstallPackages("Modules");
        }
    }
}
