using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleDefinitionRepository
    {
        IEnumerable<ModuleDefinition> GetModuleDefinitions(int sideId);
        ModuleDefinition GetModuleDefinition(int moduleDefinitionId, int sideId);
        void UpdateModuleDefinition(ModuleDefinition moduleDefinition);
        void DeleteModuleDefinition(int moduleDefinitionId, int siteId);
    }
}
