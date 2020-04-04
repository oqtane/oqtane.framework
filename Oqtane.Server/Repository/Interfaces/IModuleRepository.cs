using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IModuleRepository
    {
        IEnumerable<Module> GetModules(int siteId);
        Module AddModule(Module module);
        Module UpdateModule(Module module);
        Module GetModule(int moduleId);
        void DeleteModule(int moduleId);
        string ExportModule(int moduleId);
        bool ImportModule(int moduleId, string content);
    }
}
