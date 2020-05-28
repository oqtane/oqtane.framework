using Oqtane.Models;
using Oqtane.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IModuleDefinitionService
    {
        Task<List<ModuleDefinition>> GetModuleDefinitionsAsync(int siteId);
        Task<ModuleDefinition> GetModuleDefinitionAsync(int moduleDefinitionId, int siteId);
        Task UpdateModuleDefinitionAsync(ModuleDefinition moduleDefinition);
        Task InstallModuleDefinitionsAsync();
        Task DeleteModuleDefinitionAsync(int moduleDefinitionId, int siteId);
        Task CreateModuleDefinitionAsync(ModuleDefinition moduleDefinition, int moduleId);
    }
}
