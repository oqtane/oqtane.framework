using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleRepository
    {
        IEnumerable<Module> GetModules();
        Module AddModule(Module Module);
        Module UpdateModule(Module Module);
        Module GetModule(int ModuleId);
        void DeleteModule(int ModuleId);
    }
}
