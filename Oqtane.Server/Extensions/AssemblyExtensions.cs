using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oqtane.Shared;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetInterfaces<TInterfaceType>(this Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return assembly.GetTypes(typeof(TInterfaceType));
        }

        public static IEnumerable<Type> GetTypes(this Assembly assembly, Type interfaceType)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (interfaceType is null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return assembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(interfaceType));
        }

        public static bool IsOqtaneAssembly(this Assembly assembly)
        {
            return assembly.FullName != null && (assembly.FullName.Contains("oqtane.", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsOqtaneAssembly(this FileInfo fileInfo)
        {
            return (fileInfo.Name.Contains("oqtane.", StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Assembly> GetOqtaneAssemblies(this AppDomain appDomain)
        {
            return appDomain.GetAssemblies().Where(a => a.IsOqtaneAssembly());
        }
    }
}
