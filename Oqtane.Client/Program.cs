using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Modules;
using Oqtane.Shared;
using Oqtane.Services;

namespace Oqtane.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            HttpClient httpClient = new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};

            builder.Services.AddSingleton(httpClient);
            builder.Services.AddOptions();

            // Register localization services
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddOqtaneAuthentication();
            builder.Services.AddOqtaneServices();

            await LoadClientAssemblies(httpClient);

            // dynamically register module contexts and repository services
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var implementationTypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)));

                foreach (Type implementationtype in implementationTypes)
                {
                    Type servicetype = Type.GetType(implementationtype.AssemblyQualifiedName.Replace(implementationtype.Name, "I" + implementationtype.Name));
                    if (servicetype != null)
                    {
                        builder.Services.AddScoped(servicetype, implementationtype); // traditional service interface
                    }
                    else
                    {
                        builder.Services.AddScoped(implementationtype, implementationtype); // no interface defined for service
                    }
                }

                assembly.GetInstances<IClientStartup>()
                    .ToList()
                    .ForEach(x => x.ConfigureServices(builder.Services));
            }

            await builder.Build().RunAsync();
        }

        private static async Task LoadClientAssemblies(HttpClient http)
        {
            // get list of loaded assemblies on the client 
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToList();

            // get assemblies from server and load into client app domain
            var zip = await http.GetByteArrayAsync($"/~/api/Installation/load");

            // asemblies and debug symbols are packaged in a zip file
            using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
            {
                var dlls = new Dictionary<string, byte[]>();
                var pdbs = new Dictionary<string, byte[]>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!assemblies.Contains(Path.GetFileNameWithoutExtension(entry.Name)))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();
                            switch (Path.GetExtension(entry.Name))
                            {
                                case ".dll":
                                    // Loads the stallite assemblies early
                                    if (entry.Name.EndsWith(Constants.StalliteAssemblyExtension))
                                    {
                                        Assembly.Load(file);
                                    }
                                    else
                                    {
                                        dlls.Add(entry.Name, file);
                                    }
                                    break;
                                case ".pdb":
                                    pdbs.Add(entry.Name, file);
                                    break;
                            }
                        }
                    }
                }

                foreach (var item in dlls)
                {
                    if (pdbs.ContainsKey(item.Key))
                    {
                        Assembly.Load(item.Value, pdbs[item.Key]);
                    }
                    else
                    {
                        Assembly.Load(item.Value);
                    }
                }
            }
        }
    }
}
