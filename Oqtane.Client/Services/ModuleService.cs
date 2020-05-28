using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ModuleService : ServiceBase, IModuleService
    {
        
        private readonly SiteState _siteState;

        public ModuleService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Module");

        public async Task<List<Module>> GetModulesAsync(int siteId)
        {
            List<Module> modules = await GetJsonAsync<List<Module>>($"{Apiurl}?siteid={siteId}");
            modules = modules
                .OrderBy(item => item.Order)
                .ToList();
            return modules;
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

        public async Task<bool> ImportModuleAsync(int moduleId, string content)
        {
            return await PostJsonAsync<string,bool>($"{Apiurl}/import?moduleid={moduleId}", content);
        }

        public async Task<string> ExportModuleAsync(int moduleId)
        {
            return await GetStringAsync($"{Apiurl}/export?moduleid={moduleId}");
        }
    }
}
