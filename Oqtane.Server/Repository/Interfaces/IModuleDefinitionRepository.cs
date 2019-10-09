using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleDefinitionRepository
    {
        IEnumerable<ModuleDefinition> GetModuleDefinitions(int SideId);
        void UpdateModuleDefinition(ModuleDefinition ModuleDefinition);
        void DeleteModuleDefinition(int ModuleDefinitionId, int SiteId);

    }
}
