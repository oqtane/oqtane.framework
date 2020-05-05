using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Modules;
using Oqtane.Shared;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        private static readonly string[] BlackList = new[]
        {
            "System.",
            "Microsoft.",
            "dbup",
            "Swashbuckle.",
            "Newtonsoft.",
            "api-ms-",
            "Mono.",
            "mscor",
            "netstandard",
            "SQLite",
            "WindowsBase.dll",
            "ucrtbase",
            "dotnet-",
            "NuGet",
            "aspnetcorev2_inprocess.dll",
            "clrcompression.dll",
            "clretwrc.dll",
            "clrjit.dll",
            "coreclr.dll",
            "dbgshim.dll",
            "e_sqlite3.dll",
            "hostfxr.dll",
            "sni.dll",
            "hostpolicy.dll",
        };

        public static IServiceCollection AddOqtaneParts(this IServiceCollection services)
        {
            LoadAssemblies();
            services.AddOqtaneServices();
            return services;
        }


        private static IServiceCollection AddOqtaneServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var hostedServiceType = typeof(IHostedService);
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // dynamically register module services, contexts, and repository classes
                var implementationTypes = assembly.GetInterfaces<IService>();
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        services.AddScoped(serviceType ?? implementationType, implementationType);
                    }
                }

                // dynamically register hosted services
                var serviceTypes = assembly.GetTypes(hostedServiceType);
                foreach (var serviceType in serviceTypes)
                {
                    if (serviceType.IsSubclassOf(typeof(HostedServiceBase)))
                    {
                        services.AddSingleton(hostedServiceType, serviceType);
                    }
                }
            }

            return services;
        }

        private static void LoadAssemblies()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null) return;

            var assembliesFolder = new DirectoryInfo(assemblyPath);


            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var dll in assembliesFolder.EnumerateFiles($"*.dll", SearchOption.TopDirectoryOnly).Where(f => !f.Name.StartWithAnyOf(BlackList)))
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(dll.FullName);
                }
                catch
                {
                    Console.WriteLine($"Not Assembly : {dll.Name}");
                    continue;
                }

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (!assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())))
                {
                    try
                    {
                        var pdb = Path.ChangeExtension(dll.FullName, ".pdb");
                        Assembly assembly = null;

                        // load assembly ( and symbols ) from stream to prevent locking files ( as long as dependencies are in /bin they will load as well )
                        if (File.Exists(pdb))
                        {
                            assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)), new MemoryStream(File.ReadAllBytes(pdb)));
                        }
                        else
                        {
                            assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)));
                        }

                        Console.WriteLine($"Loaded : {assemblyName}");
                        
                        // TODO : if (!assembly.IsOqtaneAssembly()) Blacklist it 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed : {assemblyName}\n{e}");
                    }
                }
            }

            // some debug tools
            // var ass = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a=> a.FullName);
            // foreach (var assembly in ass)
            // {
            //     Console.WriteLine($"{assembly.FullName}");
            // }
        }
    }
}
