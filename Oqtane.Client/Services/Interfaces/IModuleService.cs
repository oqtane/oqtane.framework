using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IModuleService
    {
        Task<List<Module>> GetModulesAsync(int SiteId);
        Task<Module> GetModuleAsync(int ModuleId);
        Task<Module> AddModuleAsync(Module Module);
        Task<Module> UpdateModuleAsync(Module Module);
        Task DeleteModuleAsync(int ModuleId);
        Task<bool> ImportModuleAsync(int ModuleId, string Content);
        Task<string> ExportModuleAsync(int ModuleId);
    }
}
