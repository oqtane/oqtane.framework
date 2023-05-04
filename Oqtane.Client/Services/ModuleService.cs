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
    }
}
