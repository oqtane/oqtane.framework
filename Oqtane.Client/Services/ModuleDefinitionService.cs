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

        public async Task LoadModuleDefinitionsAsync(int siteId, Runtime runtime)
        {
            // get list of modules from the server
            List<ModuleDefinition> moduledefinitions = await GetModuleDefinitionsAsync(siteId);

            // download assemblies to browser when running client-side Blazor
            if (runtime == Runtime.WebAssembly)
            {
                // get list of loaded assemblies on the client ( in the client-side hosting module the browser client has its own app domain )
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (ModuleDefinition moduledefinition in moduledefinitions)
                {
                    // if a module has dependencies, check if they are loaded
                    if (moduledefinition.Dependencies != "")
                    {
                        foreach (string dependency in moduledefinition.Dependencies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string assemblyname = dependency.Replace(".dll", "");
                            if (assemblies.Where(item => item.FullName.StartsWith(assemblyname + ",")).FirstOrDefault() == null)
                            {
                                // download assembly from server and load
                                var bytes = await _http.GetByteArrayAsync($"{Apiurl}/load/{assemblyname}.dll");
                                Assembly.Load(bytes);
                            }
                        }
                    }
                    // check if the module assembly is loaded
                    if (assemblies.Where(item => item.FullName.StartsWith(moduledefinition.AssemblyName + ",")).FirstOrDefault() == null)
                    {
                        // download assembly from server and load
                        var bytes = await _http.GetByteArrayAsync($"{Apiurl}/load/{moduledefinition.AssemblyName}.dll");
                        Assembly.Load(bytes);
                    }
                }
            }
        }
        public async Task CreateModuleDefinitionAsync(ModuleDefinition moduleDefinition, int moduleId)
        {
            await PostJsonAsync($"{Apiurl}?moduleid={moduleId.ToString()}", moduleDefinition);
        }
    }
}
