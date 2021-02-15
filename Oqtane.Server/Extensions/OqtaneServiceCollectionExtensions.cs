using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        public static IServiceCollection AddOqtane(this IServiceCollection services, Runtime runtime, string[] supportedCultures)
        {
            LoadAssemblies();
            LoadSatelliteAssemblies(supportedCultures);
            services.AddOqtaneServices(runtime);

            return services;
        }

        private static IServiceCollection AddOqtaneServices(this IServiceCollection services, Runtime runtime)
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

                // register server startup services
                var startUps = assembly.GetInstances<IServerStartup>();
                foreach (var startup in startUps)
                {
                    startup.ConfigureServices(services);
                }

                if (runtime == Runtime.Server)
                {
                    // register client startup services if running on server
                    assembly.GetInstances<IClientStartup>()
                        .ToList()
                        .ForEach(x => x.ConfigureServices(services));
                }
            }
            return services;
        }

        private static void LoadAssemblies()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null) return;

            AssemblyLoadContext.Default.Resolving += ResolveDependencies;

            var assembliesFolder = new DirectoryInfo(assemblyPath);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var dll in assembliesFolder.EnumerateFiles($"*.dll", SearchOption.TopDirectoryOnly).Where(f => f.IsOqtaneAssembly()))
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed : {assemblyName}\n{e}");
                    }
                }
            }
        }

        private static void LoadSatelliteAssemblies(string[] supportedCultures)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null)
            {
                return;
            }

            AssemblyLoadContext.Default.Resolving += ResolveDependencies;

            foreach (var culture in supportedCultures)
            {
                if (culture == Constants.DefaultCulture)
                {
                    continue;
                }

                var assembliesFolder = new DirectoryInfo(Path.Combine(assemblyPath, culture));
                if (assembliesFolder.Exists)
                {
                    foreach (var assemblyFile in assembliesFolder.EnumerateFiles(Constants.SatelliteAssemblyExtension))
                    {
                        AssemblyName assemblyName;
                        try
                        {
                            assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                        }
                        catch
                        {
                            Console.WriteLine($"Not Satellite Assembly : {assemblyFile.Name}");
                            continue;
                        }

                        try
                        {
                            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyFile.FullName)));
                            Console.WriteLine($"Loaded : {assemblyName}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed : {assemblyName}\n{e}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"The satellite assemblies folder named '{culture}' is not found.");
                }
            }
        }

        private static Assembly ResolveDependencies(AssemblyLoadContext context, AssemblyName name)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + Path.DirectorySeparatorChar + name.Name + ".dll";
            if (File.Exists(assemblyPath))
            {
                return context.LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyPath)));
            }
            else
            {
                return null;
            }
        }

    }
}
