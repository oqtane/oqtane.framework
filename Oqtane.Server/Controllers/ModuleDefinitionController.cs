using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleDefinitionController : Controller
    {
        private readonly IModuleDefinitionRepository moduledefinitions;

        public ModuleDefinitionController(IModuleDefinitionRepository ModuleDefinitions)
        {
            moduledefinitions = ModuleDefinitions;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ModuleDefinition> Get()
        {
            return moduledefinitions.GetModuleDefinitions();
        }
    }
}
