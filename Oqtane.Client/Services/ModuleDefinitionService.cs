using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System;
using System.Reflection;

namespace Oqtane.Services
{
    public class ModuleDefinitionService : ServiceBase, IModuleDefinitionService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        private List<ModuleDefinition> moduledefinitions;

        public ModuleDefinitionService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "ModuleDefinition");
        }

        public async Task<List<ModuleDefinition>> GetModuleDefinitionsAsync()
        {
            if (moduledefinitions == null)
            {
                moduledefinitions = await http.GetJsonAsync<List<ModuleDefinition>>(apiurl);

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
            }
            return moduledefinitions.OrderBy(item => item.Name).ToList();
        }
    }
}
