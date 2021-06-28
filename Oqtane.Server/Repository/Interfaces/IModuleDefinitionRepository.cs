using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleDefinitionRepository
    {
        IEnumerable<ModuleDefinition> GetModuleDefinitions();
        IEnumerable<ModuleDefinition> GetModuleDefinitions(int siteId);
        ModuleDefinition GetModuleDefinition(int moduleDefinitionId, int siteId);
        ModuleDefinition GetModuleDefinition(int moduleDefinitionId, bool tracking);
        void UpdateModuleDefinition(ModuleDefinition moduleDefinition);
        void DeleteModuleDefinition(int moduleDefinitionId);
    }
}
