using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository ModuleDefinitions;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions)
        {
            this.ModuleDefinitions = ModuleDefinitions;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get()
        {
            return ModuleDefinitions.GetModuleDefinitions();
        }
    }
}
