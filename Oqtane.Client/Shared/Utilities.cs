using System;
using Oqtane.Models;

namespace Oqtane.Shared
{
    public class Utilities
    {

        public static string NavigateUrl(string alias, string path, string parameters)
        {
            string url = "";
            if (alias != "")
            {
                url += alias + "/";
            }
            if (path != "" && path != "/")
            {
                url += path + "/";
            }
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                url += "?" + parameters;
            }
            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }
            return url;
        }

        public static string EditUrl(string alias, string path, int moduleid, string action, string parameters)
        {
            string url = NavigateUrl(alias, path, "");
            if (url == "/") url = "";
            if (moduleid != -1)
            {
                url += "/" + moduleid.ToString();
            }
            if (moduleid != -1 && action != "")
            {
                url += "/" + action;
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                url += "?" + parameters;
            }
            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }
            return url;
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
