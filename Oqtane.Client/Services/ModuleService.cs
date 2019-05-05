using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Oqtane.Services
{
    public class ModuleService : ServiceBase, IModuleService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public ModuleService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Module");
        }

        public async Task<List<Module>> GetModulesAsync(int PageId)
        {
            List<Module> modules = await http.GetJsonAsync<List<Module>>(apiurl + "?pageid=" + PageId.ToString());
            modules = modules
                .OrderBy(item => item.Order)
                .ToList();
            return modules;
        }

        public async Task<List<Module>> GetModulesAsync(int SiteId, string ModuleDefinitionName)
        {
            List<Module> modules = await http.GetJsonAsync<List<Module>>(apiurl + "?siteid=" + SiteId.ToString() + "&moduledefinitionname=" + ModuleDefinitionName);
            return modules.ToList();
        }

        public async Task AddModuleAsync(Module module)
        {
            await http.PostJsonAsync(apiurl, module);
        }

        public async Task UpdateModuleAsync(Module module)
        {
            await http.PutJsonAsync(apiurl + "/" + module.ModuleId.ToString(), module);
        }

        public async Task DeleteModuleAsync(int ModuleId)
        {
            await http.DeleteAsync(apiurl + "/" + ModuleId.ToString());
        }
    }
}
