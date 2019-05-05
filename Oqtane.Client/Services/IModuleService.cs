using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IModuleService
    {
        Task<List<Module>> GetModulesAsync(int PageId);
        Task<List<Module>> GetModulesAsync(int SiteId, string ModuleDefinitionName);
        Task AddModuleAsync(Module module);
        Task UpdateModuleAsync(Module module);
        Task DeleteModuleAsync(int ModuleId);
    }
}
