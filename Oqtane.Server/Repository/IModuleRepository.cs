using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleRepository
    {
        IEnumerable<Module> GetModules();
        IEnumerable<Module> GetModules(int SiteId, string ModuleDefinitionName);
        void AddModule(Module Module);
        void UpdateModule(Module Module);
        Module GetModule(int ModuleId);
        void DeleteModule(int ModuleId);
    }
}
