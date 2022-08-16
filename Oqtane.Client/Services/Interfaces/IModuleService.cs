using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retreive and store modules (<see cref="Module"/>)
    /// </summary>
    public interface IModuleService
    {
        /// <summary>
        /// Returns a list of modules for the given site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Module>> GetModulesAsync(int siteId);

        /// <summary>
        /// Returns a specific module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        Task<Module> GetModuleAsync(int moduleId);

        /// <summary>
        /// Adds a new module
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        Task<Module> AddModuleAsync(Module module);

        /// <summary>
        /// Updates an existing module
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        Task<Module> UpdateModuleAsync(Module module);

        /// <summary>
        /// Deletes a module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        Task DeleteModuleAsync(int moduleId);

        /// <summary>
        /// Imports a module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="content">module in JSON format</param>
        /// <returns></returns>
        Task<bool> ImportModuleAsync(int moduleId, int pageId, string content);

        /// <summary>
        /// Exports a given module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns>module in JSON</returns>
        Task<string> ExportModuleAsync(int moduleId, int pageId);
    }
}
