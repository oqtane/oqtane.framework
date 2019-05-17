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
    public class ThemeService : ServiceBase, IThemeService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        private List<Theme> themes;

        public ThemeService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Theme");
        }

        public async Task<List<Theme>> GetThemesAsync()
        {
            if (themes == null)
            {
                themes = await http.GetJsonAsync<List<Theme>>(apiurl);

                // get list of loaded assemblies
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Theme theme in themes)
                {
                    if (theme.Dependencies != "")
                    {
                        foreach (string dependency in theme.Dependencies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
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
                    if (assemblies.Where(item => item.FullName.StartsWith(theme.AssemblyName + ",")).FirstOrDefault() == null)
                    {
                        // download assembly from server and load
                        var bytes = await http.GetByteArrayAsync("_framework/_bin/" + theme.AssemblyName + ".dll");
                        Assembly.Load(bytes);
                    }
                }
            }
            return themes.OrderBy(item => item.Name).ToList();
        }
    }
}
