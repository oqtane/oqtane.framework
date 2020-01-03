using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        private static readonly IList<Assembly> _oqtaneModuleAssemblies = new List<Assembly>();

        private static Assembly[] Assemblies => AppDomain.CurrentDomain.GetAssemblies();

        internal static IEnumerable<Assembly> GetOqtaneModuleAssemblies() => _oqtaneModuleAssemblies;

        public static IServiceCollection AddOqtaneModules(this IServiceCollection services)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var assembliesFolder = new DirectoryInfo(assemblyPath);

            // iterate through Oqtane module assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var file in assembliesFolder.EnumerateFiles("*.Module.*.dll"))
            {
                // check if assembly is already loaded
                var assembly = Assemblies.Where(a => a.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                    _oqtaneModuleAssemblies.Add(assembly);
                }
            }

            return services;
        }

        public static IServiceCollection AddOqtaneThemes(this IServiceCollection services)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var assembliesFolder = new DirectoryInfo(assemblyPath);

            // iterate through Oqtane theme assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var file in assembliesFolder.EnumerateFiles("*.Theme.*.dll"))
            {
                // check if assembly is already loaded
                var assembly = Assemblies.Where(a => a.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                    _oqtaneModuleAssemblies.Add(assembly);
                }
            }

            return services;
        }

        public static IServiceCollection AddOqtaneServices(this IServiceCollection services)
        {
            // dynamically register module services, contexts, and repository classes
            var assemblies = Assemblies.
                Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (var assembly in assemblies)
            {
                var implementationTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (var implementationType in implementationTypes)
                {
                    var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, "I" + implementationType.Name));
                    if (serviceType != null)
                    {
                        services.AddScoped(serviceType, implementationType); // traditional service interface
                    }
                    else
                    {
                        services.AddScoped(implementationType, implementationType); // no interface defined for service
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddOqtaneHostedServices(this IServiceCollection services)
        {
            // dynamically register hosted services
            foreach (var assembly in Assemblies)
            {
                var serviceTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IHostedService)))
                    .ToArray();
                foreach (var serviceType in serviceTypes)
                {
                    if (serviceType.Name != nameof(HostedServiceBase))
                    {
                        services.AddSingleton(typeof(IHostedService), serviceType);
                    }
                }
            }

            return services;
        }
    }
}
