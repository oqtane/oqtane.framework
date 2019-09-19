using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IInstallation Installation;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions, IInstallation Installation)
        {
            this.ModuleDefinitions = ModuleDefinitions;
            this.Installation = Installation;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get(string siteid)
        {
            return ModuleDefinitions.GetModuleDefinitions(int.Parse(siteid));
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
            Installation.Install("Modules");
        }
    }
}
