using System;

namespace Oqtane.Shared
{
    public class Utilities
    {
        public static string GetAlias(string absoluteUri)
        {
            string alias = "";
            Uri uri = new Uri(absoluteUri);
            if (uri.AbsolutePath.StartsWith("/~"))
            {
                alias = uri.Segments[1];
            }
            return alias;
        }

        public static string GetTypeNameClass(string typename)
        {
            if (typename.Contains(","))
            {
                typename = typename.Substring(0, typename.IndexOf(","));
            }
            string[] fragments = typename.Split('.');
            return fragments[fragments.Length - 1];
        }
    }
}
