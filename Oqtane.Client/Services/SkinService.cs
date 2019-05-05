using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System;

namespace Oqtane.Services
{
    public class SkinService : ServiceBase, ISkinService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        private List<Skin> skins;

        public SkinService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Skin");
        }

        public async Task<List<Skin>> GetSkinsAsync()
        {
            if (skins == null)
            {
                skins = await http.GetJsonAsync<List<Skin>>(apiurl);

                // get list of loaded assemblies
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Skin skin in skins)
                {
                    if (skin.Dependencies != "")
                    {
                        foreach (string dependency in skin.Dependencies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
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
                    if (assemblies.Where(item => item.FullName.StartsWith(skin.AssemblyName + ",")).FirstOrDefault() == null)
                    {
                        // download assembly from server and load
                        var bytes = await http.GetByteArrayAsync("_framework/_bin/" + skin.AssemblyName + ".dll");
                        Assembly.Load(bytes);
                    }
                }
            }
            return skins.OrderBy(item => item.Name).ToList();
        }
    }
}
