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

        public ModuleDefinitionService(HttpClient http, SiteState sitestate)
        {
            this.http = http;
            this.sitestate = sitestate;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, "ModuleDefinition"); }
        }

        public async Task<List<ModuleDefinition>> GetModuleDefinitionsAsync()
        {
            List<ModuleDefinition> moduledefinitions = await http.GetJsonAsync<List<ModuleDefinition>>(apiurl);

            // get list of loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (ModuleDefinition moduledefinition in moduledefinitions)
            {
                if (moduledefinition.Dependencies != "")
                {
                    foreach (string dependency in moduledefinition.Dependencies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string assemblyname = dependency.Replace(".dll", "");
                        if (assemblies.Where(item => item.FullName.StartsWith(assemblyname + ",")).FirstOrDefault() == null)
                        {
                            // download assembly from server and load
                            var bytes = await http.GetByteArrayAsync("_framework/_bin/" + assemblyname + ".dll");
                            Assembly.Load(bytes);
                        }
                    }
                }
                if (assemblies.Where(item => item.FullName.StartsWith(moduledefinition.AssemblyName + ",")).FirstOrDefault() == null)
                {
                    // download assembly from server and load
                    var bytes = await http.GetByteArrayAsync("_framework/_bin/" + moduledefinition.AssemblyName + ".dll");
                    Assembly.Load(bytes);
                }
            }

            return moduledefinitions.OrderBy(item => item.Name).ToList();
        }
    }
}
