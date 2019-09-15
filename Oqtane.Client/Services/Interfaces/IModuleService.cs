using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IModuleService
    {
        Task<List<Module>> GetModulesAsync(int PageId);
        Task<List<Module>> GetModulesAsync(int SiteId, string ModuleDefinitionName);
        Task<Module> GetModuleAsync(int ModuleId);
        Task<Module> AddModuleAsync(Module Module);
        Task<Module> UpdateModuleAsync(Module Module);
        Task DeleteModuleAsync(int ModuleId);
    }
}
