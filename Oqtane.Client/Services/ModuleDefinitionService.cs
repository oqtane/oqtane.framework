using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Reflection;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Services
{
    public class ModuleDefinitionService : ServiceBase, IModuleDefinitionService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;

        public ModuleDefinitionService(HttpClient http, SiteState siteState) : base(http)
        {
            _http = http;
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "ModuleDefinition");

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

        public async Task InstallModuleDefinitionsAsync()
        {
            await GetJsonAsync<List<string>>($"{Apiurl}/install");
        }

        public async Task DeleteModuleDefinitionAsync(int moduleDefinitionId, int siteId)
        {
            await DeleteAsync($"{Apiurl}/{moduleDefinitionId}?siteid={siteId}");
        }

        public async Task<ModuleDefinition> CreateModuleDefinitionAsync(ModuleDefinition moduleDefinition)
        {
            return await PostJsonAsync($"{Apiurl}", moduleDefinition);
        }
    }
}
