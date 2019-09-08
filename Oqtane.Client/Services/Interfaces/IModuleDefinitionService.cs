using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IModuleDefinitionService
    {
        Task<List<ModuleDefinition>> GetModuleDefinitionsAsync();
        Task InstallModulesAsync();
     }
}
