using System;
using Oqtane.Shared;

namespace Oqtane.Models
{
    /// <summary>
    /// A route is comprised of multiple components ( some optional depending on context )
    /// {scheme}://{hostname}/{aliaspath}/{pagepath}/*/{moduleid}/{action}/!/{urlparameters}?{query}#{fragment}
    /// </summary>
    public class Route
    {
        /// <summary>
        /// default constructor
        /// the route parameter can be obtained from NavigationManager.Uri on client or HttpContext.Request.GetEncodedUrl() on server
        /// the aliaspath parameter can be obtained from PageState.Alias.Path on client or TenantManager.GetAlias().Path on server
        /// </summary>
        public Route(string route, string aliaspath)
        {
            Uri uri = new Uri(route);
            Authority = uri.Authority;
            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.Port.ToString();
            Query = uri.Query;
            Fragment = uri.Fragment;
            AbsolutePath = uri.AbsolutePath;
            PathAndQuery = uri.PathAndQuery;
            AliasPath = aliaspath;
            PagePath = AbsolutePath;
            ModuleId = "";
            Action = "";
            UrlParameters = "";

            if (AliasPath.Length != 0 && PagePath.StartsWith("/" + AliasPath))
            {
                PagePath = PagePath.Substring(AliasPath.Length + 1);
            }
            int pos = PagePath.IndexOf("/" + Constants.UrlParametersDelimiter + "/");
            if (pos != -1)
            {
                UrlParameters = PagePath.Substring(pos + 3);
                PagePath = PagePath.Substring(0, pos);
            }
            pos = PagePath.IndexOf("/" + Constants.ModuleDelimiter + "/");
            if (pos != -1)
            {
                ModuleId = PagePath.Substring(pos + 3);
                PagePath = PagePath.Substring(0, pos);
            }
            if (ModuleId.Length != 0)
            {
                pos = ModuleId.IndexOf("/");
                if (pos != -1)
                {
                    Action = ModuleId.Substring(pos + 1); 
                    ModuleId = ModuleId.Substring(0, pos);
                }
            }
            if (PagePath.StartsWith("/"))
            {
                PagePath = (PagePath.Length == 1) ? "" : PagePath.Substring(1);
            }
            if (PagePath.EndsWith("/"))
            {
                PagePath = PagePath.Substring(0, PagePath.Length - 1);
            }
        }

        /// <summary>
        /// The host name or IP address and port number (does not include scheme or trailing slash)
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// A fully qualified route contains a scheme (ie. http, https )
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// A fully qualified route contains a host name. The host name may include a port number.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// A host name may contain a port number
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// The absolute path for the route
        /// </summary>
        public string AbsolutePath { get; set; }

        /// <summary>
        /// The absolute path for the route including the querystring
        /// </summary>
        public string PathAndQuery { get; set; }

        /// <summary>
        /// An absolute path may contain an alias path
        /// </summary>
        public string AliasPath { get; set; }

        /// <summary>
        /// A absolute path may contain a page path.
        /// </summary>
        public string PagePath { get; set; }

        /// <summary>
        /// A route may contain a module id (ie. when created using EditUrl) located after the module delimiter segment (/*/).  
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// A route may contain an action (ie. when created using EditUrl) located after the module id. 
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// A route may contain parameters located after the url parameter delimiter segment (/!/).
        /// </summary>
        public string UrlParameters { get; set; }

        /// <summary>
        /// A route may contain querystring parameters located after the ? delimiter
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// A route may contain a fragment located after the # delimiter
        /// </summary>
        public string Fragment { get; set; }

        /// <summary>
        /// The root url contains the resource identifier for the root of an Oqtane installation ( including scheme )
        /// </summary>
        public string RootUrl
        {
            get
            {
                return Scheme + "://" + Authority;
            }
        }

        /// <summary>
        /// The site url contains the resource identifier for the home page of a specific Oqtane site ( including scheme and possibly an alias path )
        /// </summary>
        public string SiteUrl
        {
            get
            {
                return Scheme + "://" + Authority + ((!string.IsNullOrEmpty(AliasPath)) ? "/" + AliasPath : "");
            }
        }
    }
}
