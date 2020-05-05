using System.Collections.Generic;
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
            return Attribute.IsDefined(assembly, typeof(OqtaneAssemblyAttribute))
//                || assembly.FullName.Contains(".Views")

                // can be added for backward compatibility                
//                || assembly.FullName.StartsWith("Oqtane.")
//                || assembly.FullName.Contains(".Module.")
//                || assembly.FullName.Contains(".Theme.")
                ;
        }

        public static IEnumerable<Assembly> GetOqtaneAssemblies(this AppDomain appDomain)
        {
            return appDomain.GetAssemblies().Where(a => a.IsOqtaneAssembly());
        }
        
    }
}
