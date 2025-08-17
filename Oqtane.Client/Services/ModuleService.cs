using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;
using Oqtane.Modules.Controls;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and store modules (<see cref="Module"/>)
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
        /// <param name="pageId"></param>
        /// <returns>module content in JSON format</returns>
        Task<string> ExportModuleAsync(int moduleId, int pageId);

        /// <summary>
        /// Exports a given module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="pageId"></param>
        /// <param name="folderId"></param>
        /// <param name="filename"></param>
        /// <returns>file id</returns>
        Task<int> ExportModuleAsync(int moduleId, int pageId, int folderId, string filename);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ModuleService : ServiceBase, IModuleService
    {
        public ModuleService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Module");

        public async Task<List<Module>> GetModulesAsync(int siteId)
        {
            return await GetJsonAsync<List<Module>>($"{Apiurl}?siteid={siteId}");
        }

        public async Task<Module> GetModuleAsync(int moduleId)
        {
            return await GetJsonAsync<Module>($"{Apiurl}/{moduleId}");
        }

        public async Task<Module> AddModuleAsync(Module module)
        {
            return await PostJsonAsync<Module>(Apiurl, module);
        }

        public async Task<Module> UpdateModuleAsync(Module module)
        {
            return await PutJsonAsync<Module>($"{Apiurl}/{module.ModuleId}", module);
        }

        public async Task DeleteModuleAsync(int moduleId)
        {
            await DeleteAsync($"{Apiurl}/{moduleId.ToString()}");
        }

        public async Task<bool> ImportModuleAsync(int moduleId, int pageId, string content)
        {
            return await PostJsonAsync<string,bool>($"{Apiurl}/import?moduleid={moduleId}&pageid={pageId}", content);
        }

        public async Task<string> ExportModuleAsync(int moduleId, int pageId)
        {
            return await GetStringAsync($"{Apiurl}/export?moduleid={moduleId}&pageid={pageId}");
        }

        public async Task<int> ExportModuleAsync(int moduleId, int pageId, int folderId, string filename)
        {
            return await PostJsonAsync<string,int>($"{Apiurl}/export?moduleid={moduleId}&pageid={pageId}&folderid={folderId}&filename={filename}", null);
        }
    }
}
