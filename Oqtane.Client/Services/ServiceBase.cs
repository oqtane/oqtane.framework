using System;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ServiceBase
    {
        // method for alias agnostic api call
        public string CreateApiUrl(string absoluteUri, string serviceName)
        {
            Uri uri = new Uri(absoluteUri);
            string apiurl = uri.Scheme + "://" + uri.Authority + "/~/api/" + serviceName;
            return apiurl;
        }

        // method for alias specific api call
        public string CreateApiUrl(Alias alias, string serviceName)
        {
            string apiurl = alias.Url + "/";
            if (alias.Path == "")
            {
                apiurl += "~/";
            }
            apiurl += "api/" + serviceName;
            return apiurl;
        }
    }
}
