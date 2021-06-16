using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var httpClient = new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};

            builder.Services.AddSingleton(httpClient);
            builder.Services.AddOptions();

            // Register localization services
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // register auth services
            builder.Services.AddOqtaneAuthorization();

            // register scoped core services
            builder.Services.AddOqtaneScopedServices();

            await LoadClientAssemblies(httpClient);

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

            ServiceActivator.Configure(host.Services);

            await host.RunAsync();
        }

        private static async Task LoadClientAssemblies(HttpClient http)
        {
            // get list of loaded assemblies on the client
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToList();

            // get assemblies from server and load into client app domain
            var zip = await http.GetByteArrayAsync($"/api/Installation/load");

            // asemblies and debug symbols are packaged in a zip file
            using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
            {
                var dlls = new Dictionary<string, byte[]>();
                var pdbs = new Dictionary<string, byte[]>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!assemblies.Contains(Path.GetFileNameWithoutExtension(entry.FullName)))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();
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

                foreach (var item in dlls)
                {
                    if (pdbs.ContainsKey(item.Key))
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value), new MemoryStream(pdbs[item.Key]));
                    }
                    else
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value));
                    }
                }
            }
        }

        private static void RegisterModuleServices(Assembly assembly, IServiceCollection services)
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

        private static void RegisterClientStartups(Assembly assembly, IServiceCollection services)
        {
            var startUps = assembly.GetInstances<IClientStartup>();
            foreach (var startup in startUps)
            {
                startup.ConfigureServices(services);
            }
        }

        private static async Task SetCultureFromLocalizationCookie(IServiceProvider serviceProvider)
        {
            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            var interop = new Interop(jsRuntime);
            var localizationCookie = await interop.GetCookie(CookieRequestCultureProvider.DefaultCookieName);
            var culture = CookieRequestCultureProvider.ParseCookieValue(localizationCookie)?.UICultures?[0].Value;
            var localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
            var cultures = await localizationService.GetCulturesAsync();

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
