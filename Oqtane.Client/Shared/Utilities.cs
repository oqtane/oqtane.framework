using System;
using Oqtane.Models;

namespace Oqtane.Shared
{
    public class Utilities
    {
        public static string NavigateUrl(PageState pagestate)
        {
            return NavigateUrl(pagestate, pagestate.Page.Path, false);
        }

        public static string NavigateUrl(PageState pagestate, bool reload)
        {
            return NavigateUrl(pagestate, pagestate.Page.Path, reload);
        }

        public static string NavigateUrl(PageState pagestate, string path)
        {
            return NavigateUrl(pagestate, path, false);
        }

        public static string NavigateUrl(PageState pagestate, string path, bool reload)
        {
            string url = pagestate.Alias.Path + "/" + path;
            if (reload)
            {
                if (url.Contains("?"))
                {
                    url += "&reload=true";
                }
                else
                {
                    url += "?reload=true";
                }
            }
            return url;
        }

        public static string EditUrl(PageState pagestate, Module modulestate, string action)
        {
            return EditUrl(pagestate, modulestate, action, "");
        }

        public static string EditUrl(PageState pagestate, Module modulestate, string action, string parameters)
        {
            string url = pagestate.Alias.Path;
            if (pagestate.Page.Path != "")
            {
                url += "/" + pagestate.Page.Path;
            }
            url += "/" + modulestate.ModuleId.ToString();
            if (action != "")
            {
                url += "/" + action;
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                url += "?" + parameters;
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
