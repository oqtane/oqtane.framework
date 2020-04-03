using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Services.Interfaces;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ModuleService : ServiceBase, IModuleService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public ModuleService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Module"); }
        }

        public async Task<List<Module>> GetModulesAsync(int siteId)
        {
            List<Module> modules = await _http.GetJsonAsync<List<Module>>($"{Apiurl}?siteid={siteId.ToString()}");
            modules = modules
                .OrderBy(item => item.Order)
                .ToList();
            return modules;
        }

        public async Task<Module> GetModuleAsync(int moduleId)
        {
            return await _http.GetJsonAsync<Module>($"{Apiurl}/{moduleId.ToString()}");
        }

        public async Task<Module> AddModuleAsync(Module module)
        {
            return await _http.PostJsonAsync<Module>(Apiurl, module);
        }

        public async Task<Module> UpdateModuleAsync(Module module)
        {
            return await _http.PutJsonAsync<Module>($"{Apiurl}/{module.ModuleId.ToString()}", module);
        }

        public async Task DeleteModuleAsync(int moduleId)
        {
            await _http.DeleteAsync($"{Apiurl}/{moduleId.ToString()}");
        }

        public async Task<bool> ImportModuleAsync(int moduleId, string content)
        {
            return await _http.PostJsonAsync<bool>($"{Apiurl}/import?moduleid={moduleId}", content);
        }

        public async Task<string> ExportModuleAsync(int moduleId)
        {
            return await _http.GetStringAsync($"{Apiurl}/export?moduleid={moduleId.ToString()}");
        }
    }
}
