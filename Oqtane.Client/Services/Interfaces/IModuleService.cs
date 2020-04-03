using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IModuleService
    {
        Task<List<Module>> GetModulesAsync(int siteId);
        Task<Module> GetModuleAsync(int moduleId);
        Task<Module> AddModuleAsync(Module module);
        Task<Module> UpdateModuleAsync(Module module);
        Task DeleteModuleAsync(int moduleId);
        Task<bool> ImportModuleAsync(int moduleId, string content);
        Task<string> ExportModuleAsync(int moduleId);
    }
}
