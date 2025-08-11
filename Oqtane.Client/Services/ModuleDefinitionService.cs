using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ModuleDefinitionService : ServiceBase, IModuleDefinitionService
    {
        public ModuleDefinitionService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("ModuleDefinition");

        public async Task<List<ModuleDefinition>> GetModuleDefinitionsAsync(int siteId)
        {
            List<ModuleDefinition> moduledefinitions = await GetJsonAsync<List<ModuleDefinition>>($"{Apiurl}?siteid={siteId}");
            return moduledefinitions.OrderBy(item => item.Name).ToList();
        }

        public async Task<ModuleDefinition> GetModuleDefinitionAsync(int moduleDefinitionId, int siteId)
        {
            return await GetJsonAsync<ModuleDefinition>($"{Apiurl}/{moduleDefinitionId}?siteid={siteId}");
        }

        public async Task UpdateModuleDefinitionAsync(ModuleDefinition moduleDefinition)
        {
            await PutJsonAsync($"{Apiurl}/{moduleDefinition.ModuleDefinitionId}", moduleDefinition);
        }

        public async Task DeleteModuleDefinitionAsync(int moduleDefinitionId, int siteId)
        {
            await DeleteAsync($"{Apiurl}/{moduleDefinitionId}?siteid={siteId}");
        }

        public async Task<ModuleDefinition> CreateModuleDefinitionAsync(ModuleDefinition moduleDefinition)
        {
            return await PostJsonAsync($"{Apiurl}", moduleDefinition);
        }

        public async Task<List<Template>> GetModuleDefinitionTemplatesAsync()
        {
            List<Template> templates = await GetJsonAsync<List<Template>>($"{Apiurl}/templates");
            return templates;
        }
    }
}
