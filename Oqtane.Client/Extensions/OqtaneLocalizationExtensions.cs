using System;

namespace Microsoft.Extensions.Localization
{
    public static class OqtaneLocalizationExtensions
    {
        /// <summary>
        /// Gets the string resource for the specified key and returns the value if the resource does not exist
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="key">the static key used to identify the string resource</param>
        /// <param name="value">the default value if the resource for the static key does not exist</param>
        /// <returns></returns>
        public static string GetString(this IStringLocalizer localizer, string key, string value)
        {
            string localizedValue = localizer[key];
            if (localizedValue == key && !string.IsNullOrEmpty(value)) // not localized
            {
                localizedValue = value;
            }
            return localizedValue;
        }

        /// <summary>
        /// Creates an IStringLocalizer based on a type name. This extension method is useful in scenarios where the default IStringLocalizer is unable to locate the resources.
        /// </summary>
        /// <param name="localizerFactory"></param>
        /// <param name="fullTypeName">the full type name ie. GetType().FullName</param>
        /// <returns></returns>
        public static IStringLocalizer Create(this IStringLocalizerFactory localizerFactory, string fullTypeName)
        {
            var typename = fullTypeName;

            // handle generic types
            var type = Type.GetType(fullTypeName);
            if (type.IsGenericType)
            {
                typename = type.GetGenericTypeDefinition().FullName;
                typename = typename.Substring(0, typename.IndexOf("`")); // remove generic type info
            }

            // format typename
            if (typename.Contains(","))
            {
                typename = typename.Substring(0, typename.IndexOf(",")); // remove assembly info
            }

            // remove rootnamespace
            var rootnamespace = "";
            var attributes = type.Assembly.GetCustomAttributes(typeof(RootNamespaceAttribute), false);
            if (attributes.Length > 0)
            {
                rootnamespace = ((RootNamespaceAttribute)attributes[0]).RootNamespace;
            }
            typename = typename.Replace(rootnamespace + ".", "");

            // create IStringLocalizer using factory
            return localizerFactory.Create(typename, type.Assembly.GetName().Name);
        }    
    }
}
