using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Oqtane.Documentation;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Client
{
    [PrivateApi("Mark Entry-Program as private, since it's not very useful in the public docs")]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var httpClient = new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};
            builder.Services.AddSingleton(httpClient);            
            builder.Services.AddHttpClient(); // IHttpClientFactory for calling remote services via RemoteServiceBase

            builder.Services.AddOptions();

            // register localization services
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // register auth services
            builder.Services.AddOqtaneAuthorization();

            // register scoped core services
            builder.Services.AddOqtaneScopedServices();

            var serviceProvider = builder.Services.BuildServiceProvider();

            await LoadClientAssemblies(httpClient, serviceProvider);

            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // dynamically register module services
                RegisterModuleServices(assembly, builder.Services);

                // register client startup services
                RegisterClientStartups(assembly, builder.Services);
            }

            var host = builder.Build();

            await SetCultureFromLocalizationCookie(host.Services);

            await host.RunAsync();
        }

        private static async Task LoadClientAssemblies(HttpClient http, IServiceProvider serviceProvider)
        {
            var dlls = new Dictionary<string, byte[]>();
            var pdbs = new Dictionary<string, byte[]>();
            var list = new List<string>();

            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            var interop = new Interop(jsRuntime);
            var files = await interop.GetIndexedDBKeys(".dll");

            if (files.Count() != 0)
            {
                // get list of assemblies from server
                var json = await http.GetStringAsync("/api/Installation/list");
                var assemblies = JsonSerializer.Deserialize<List<string>>(json);

                // determine which assemblies need to be downloaded
                foreach (var assembly in assemblies)
                {
                    var file = files.FirstOrDefault(item => item.Contains(assembly));
                    if (file == null)
                    {
                        list.Add(assembly);
                    }
                    else
                    {
                        // check if newer version available
                        if (GetFileDate(assembly) > GetFileDate(file))
                        {
                            list.Add(assembly);
                        }
                    }
                }

                // get assemblies already downloaded
                foreach (var file in files)
                {
                    if (assemblies.Contains(file) && !list.Contains(file))
                    {
                        try
                        {
                            dlls.Add(file, await interop.GetIndexedDBItem<byte[]>(file));
                            var pdb = file.Replace(".dll", ".pdb");
                            if (files.Contains(pdb))
                            {
                                pdbs.Add(pdb, await interop.GetIndexedDBItem<byte[]>(pdb));
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    else // file is deprecated
                    {
                        try
                        {
                            await interop.RemoveIndexedDBItem(file);
                            await interop.RemoveIndexedDBItem(file.Replace(".dll", ".pdb"));
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
            else
            {
                list.Add("*");
            }

            if (list.Count != 0)
            {
                // get assemblies from server and load into client app domain
                var zip = await http.GetByteArrayAsync($"/api/Installation/load?list=" + string.Join(",", list));

                // asemblies and debug symbols are packaged in a zip file
                using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();

                            // save assembly to indexeddb
                            try
                            {
                                await interop.SetIndexedDBItem(entry.FullName, file);
                            }
                            catch
                            {
                                // ignore
                            }

                            switch (Path.GetExtension(entry.FullName))
                            {
                                case ".dll":
                                    dlls.Add(entry.FullName, file);
                                    break;
                                case ".pdb":
                                    pdbs.Add(entry.FullName, file);
                                    break;
                            }
                        }
                    }
                }
            }

            // load assemblies into app domain
            foreach (var item in dlls)
            {
                if (pdbs.ContainsKey(item.Key.Replace(".dll", ".pdb")))
                {
                    AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value), new MemoryStream(pdbs[item.Key.Replace(".dll", ".pdb")]));
                }
                else
                {
                    AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value));
                }
            }
        }

        private static DateTime GetFileDate(string filepath)
        {
            var segments = filepath.Split('.');
            return DateTime.ParseExact(segments[segments.Length - 2], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        private static void RegisterModuleServices(Assembly assembly, IServiceCollection services)
        {
            // dynamically register module scoped services
            try
            {
                var implementationTypes = assembly.GetInterfaces<IService>();
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        services.AddScoped(serviceType ?? implementationType, implementationType);
                    }
                }
            }
            catch
            {
                // could not interrogate assembly - likely missing dependencies
            }
        }

        private static void RegisterClientStartups(Assembly assembly, IServiceCollection services)
        {
            try
            {
                var startUps = assembly.GetInstances<IClientStartup>();
                foreach (var startup in startUps)
                {
                    startup.ConfigureServices(services);
                }
            }
            catch
            {
                // could not interrogate assembly - likely missing dependencies
            }
        }

        private static async Task SetCultureFromLocalizationCookie(IServiceProvider serviceProvider)
        {
            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            var interop = new Interop(jsRuntime);
            var localizationCookie = await interop.GetCookie(CookieRequestCultureProvider.DefaultCookieName);
            var culture = CookieRequestCultureProvider.ParseCookieValue(localizationCookie)?.UICultures?[0].Value;
            var localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
            var cultures = await localizationService.GetCulturesAsync(false);

            if (culture == null || !cultures.Any(c => c.Name.Equals(culture, StringComparison.OrdinalIgnoreCase)))
            {
                culture = cultures.Single(c => c.IsDefault).Name;
            }

            SetCulture(culture);
        }

        private static void SetCulture(string culture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
