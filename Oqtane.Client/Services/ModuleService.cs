using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ModuleService : ServiceBase, IModuleService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public ModuleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Module"); }
        }

        public async Task<List<Module>> GetModulesAsync(int SiteId)
        {
            List<Module> modules = await http.GetJsonAsync<List<Module>>(apiurl + "?siteid=" + SiteId.ToString());
            modules = modules
                .OrderBy(item => item.Order)
                .ToList();
            return modules;
        }

        public async Task<Module> GetModuleAsync(int ModuleId)
        {
            return await http.GetJsonAsync<Module>(apiurl + "/" + ModuleId.ToString());
        }

        public async Task<Module> AddModuleAsync(Module Module)
        {
            return await http.PostJsonAsync<Module>(apiurl, Module);
        }

        public async Task<Module> UpdateModuleAsync(Module Module)
        {
            return await http.PutJsonAsync<Module>(apiurl + "/" + Module.ModuleId.ToString(), Module);
        }

        public async Task DeleteModuleAsync(int ModuleId)
        {
            await http.DeleteAsync(apiurl + "/" + ModuleId.ToString());
        }

        public async Task<bool> ImportModuleAsync(int ModuleId, string Content)
        {
            return await http.PostJsonAsync<bool>(apiurl + "/import?moduleid=" + ModuleId, Content);
        }

        public async Task<string> ExportModuleAsync(int ModuleId)
        {
            return await http.GetStringAsync(apiurl + "/export?moduleid=" + ModuleId.ToString());
        }
    }
}
