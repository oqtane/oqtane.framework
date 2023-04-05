using Oqtane.Models;
using Oqtane.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{

    /// <summary>
    /// Service to manage a <see cref="ModuleDefinition"/>
    /// </summary>
    public interface IModuleDefinitionService
    {
        /// <summary>
        /// Returns a list of module definitions for the given site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<ModuleDefinition>> GetModuleDefinitionsAsync(int siteId);

        /// <summary>
        /// Returns a specific module definition
        /// </summary>
        /// <param name="moduleDefinitionId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<ModuleDefinition> GetModuleDefinitionAsync(int moduleDefinitionId, int siteId);

        /// <summary>
        /// Updates a existing module definition
        /// </summary>
        /// <param name="moduleDefinition"></param>
        /// <returns></returns>
        Task UpdateModuleDefinitionAsync(ModuleDefinition moduleDefinition);

        /// <summary>
        /// Deletes a module definition
        /// </summary>
        /// <param name="moduleDefinitionId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task DeleteModuleDefinitionAsync(int moduleDefinitionId, int siteId);

        /// <summary>
        /// Creates a new module definition
        /// </summary>
        /// <param name="moduleDefinition"></param>
        /// <returns></returns>
        Task<ModuleDefinition> CreateModuleDefinitionAsync(ModuleDefinition moduleDefinition);

        /// <summary>
        /// Returns a list of module definition templates
        /// </summary>
        /// <returns></returns>
        Task<List<Template>> GetModuleDefinitionTemplatesAsync();
    }
}
