using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System;
using System.Reflection;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ModuleDefinitionService : ServiceBase, IModuleDefinitionService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public ModuleDefinitionService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "ModuleDefinition"); }
        }

        public async Task<List<ModuleDefinition>> GetModuleDefinitionsAsync(int SiteId)
        {
            // get list of modules from the server
            List<ModuleDefinition> moduledefinitions = await http.GetJsonAsync<List<ModuleDefinition>>(apiurl + "?siteid=" + SiteId.ToString());

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
                            var bytes = await http.GetByteArrayAsync(apiurl + "/" + assemblyname + ".dll");
                            Assembly.Load(bytes);
                        }
                    }
                }
                // check if the module assembly is loaded
                if (assemblies.Where(item => item.FullName.StartsWith(moduledefinition.AssemblyName + ",")).FirstOrDefault() == null)
                {
                    // download assembly from server and load
                    var bytes = await http.GetByteArrayAsync(apiurl + "/" + moduledefinition.AssemblyName + ".dll");
                    Assembly.Load(bytes);
                }
            }

            return moduledefinitions.OrderBy(item => item.Name).ToList();
        }

        public async Task UpdateModuleDefinitionAsync(ModuleDefinition ModuleDefinition)
        {
            await http.PutJsonAsync(apiurl + "/" + ModuleDefinition.ModuleDefinitionId.ToString(), ModuleDefinition);
        }

        public async Task InstallModuleDefinitionsAsync()
        {
            await http.GetJsonAsync<List<string>>(apiurl + "/install");
        }

        public async Task DeleteModuleDefinitionAsync(int ModuleDefinitionId, int SiteId)
        {
            await http.DeleteAsync(apiurl + "/" + ModuleDefinitionId.ToString() + "?siteid=" + SiteId.ToString());
        }
    }
}
