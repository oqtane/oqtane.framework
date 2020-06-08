using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.Themes;

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
                //.Where(t => t.GetInterfaces().Contains(interfaceType));
                .Where(x => !x.IsInterface && !x.IsAbstract && interfaceType.IsAssignableFrom(x));
        }

        public static IEnumerable<Type> GetTypes<T>(this Assembly assembly)
        {
            return assembly.GetTypes(typeof(T));
        }

        public static IEnumerable<T> GetInstances<T>(this Assembly assembly) where T : class
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            var type = typeof(T);
            var list = assembly.GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericType && type.IsAssignableFrom(x));

            foreach (var type1 in list)
            {
                if (Activator.CreateInstance(type1) is T instance) yield return instance;
            }
        }

        public static bool IsOqtaneAssembly(this Assembly assembly)
        {
            return assembly.FullName != null && (assembly.FullName.Contains("oqtane", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsOqtaneAssembly(this FileInfo fileInfo)
        {
            return (fileInfo.Name.Contains("oqtane", StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Assembly> GetOqtaneAssemblies(this AppDomain appDomain)
        {
            return appDomain.GetAssemblies().Where(a => a.IsOqtaneAssembly());
        }
        public static IEnumerable<Assembly> GetOqtaneClientAssemblies(this AppDomain appDomain)
        {
            return appDomain.GetOqtaneAssemblies()
                .Where(a => a.GetTypes<IModuleControl>().Any() || a.GetTypes<IThemeControl>().Any() || a.GetTypes<IClientStartup>().Any())
                .Where(a => Utilities.GetFullTypeName(a.GetName().Name) != "Oqtane.Client");
        }

        /// <summary>
        /// Checks if type should be ignored by oqtane dynamic loader 
        /// </summary>
        /// <param name="type">Checked type</param>
        /// <returns></returns>
        public static bool IsOqtaneIgnore(this Type type)
        {
            return Attribute.IsDefined(type, typeof(OqtaneIgnoreAttribute)) || type.IsAbstract || type.IsGenericType;
        }
    }
}
