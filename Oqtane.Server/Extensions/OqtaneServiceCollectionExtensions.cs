using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Modules;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        private static readonly IList<Assembly> OqtaneModuleAssemblies = new List<Assembly>();

        private static Assembly[] Assemblies => AppDomain.CurrentDomain.GetAssemblies();

        internal static IEnumerable<Assembly> GetOqtaneModuleAssemblies() => OqtaneModuleAssemblies;

        public static IServiceCollection AddOqtaneModules(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            LoadAssemblies("Module");

            return services;
        }

        public static IServiceCollection AddOqtaneThemes(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            LoadAssemblies("Theme");

            return services;
        }

        public static IServiceCollection AddOqtaneSiteTemplates(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            LoadAssemblies("SiteTemplate");

            return services;
        }

        public static IServiceCollection AddOqtaneServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // dynamically register module services, contexts, and repository classes
            var assemblies = Assemblies.Where(item => item.FullName != null && (item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module."))).ToArray();
            foreach (var assembly in assemblies)
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

            return services;
        }

        public static IServiceCollection AddOqtaneHostedServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // dynamically register hosted services
            var hostedServiceType = typeof(IHostedService);
            foreach (var assembly in Assemblies)
            {
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

        private static void LoadAssemblies(string pattern)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null) return;

            var assembliesFolder = new DirectoryInfo(assemblyPath);

            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var dll in assembliesFolder.EnumerateFiles($"*.{pattern}.*.dll"))
            {
                // check if assembly is already loaded
                var assembly = Assemblies.FirstOrDefault(a =>!a.IsDynamic && a.Location == dll.FullName);
                if (assembly == null)
                {
                    // load assembly ( and symbols ) from stream to prevent locking files ( as long as dependencies are in /bin they will load as well )
                    string pdb = dll.FullName.Replace(".dll", ".pdb");
                    if (File.Exists(pdb))
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)), new MemoryStream(File.ReadAllBytes(pdb)));
                    }
                    else
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)));
                    }
                    if (pattern == "Module")
                    {
                        // build a list of module assemblies
                        OqtaneModuleAssemblies.Add(assembly);
                    }
                }
            }
        }
    }
}
