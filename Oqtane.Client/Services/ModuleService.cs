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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public ModuleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Module"); }
        }

        public async Task<List<Module>> GetModulesAsync(int SiteId)
        {
            List<Module> modules = await _http.GetJsonAsync<List<Module>>(apiurl + "?siteid=" + SiteId.ToString());
            modules = modules
                .OrderBy(item => item.Order)
                .ToList();
            return modules;
        }

        public async Task<Module> GetModuleAsync(int ModuleId)
        {
            return await _http.GetJsonAsync<Module>(apiurl + "/" + ModuleId.ToString());
        }

        public async Task<Module> AddModuleAsync(Module Module)
        {
            return await _http.PostJsonAsync<Module>(apiurl, Module);
        }

        public async Task<Module> UpdateModuleAsync(Module Module)
        {
            return await _http.PutJsonAsync<Module>(apiurl + "/" + Module.ModuleId.ToString(), Module);
        }

        public async Task DeleteModuleAsync(int ModuleId)
        {
            await _http.DeleteAsync(apiurl + "/" + ModuleId.ToString());
        }

        public async Task<bool> ImportModuleAsync(int ModuleId, string Content)
        {
            return await _http.PostJsonAsync<bool>(apiurl + "/import?moduleid=" + ModuleId, Content);
        }

        public async Task<string> ExportModuleAsync(int ModuleId)
        {
            return await _http.GetStringAsync(apiurl + "/export?moduleid=" + ModuleId.ToString());
        }
    }
}
