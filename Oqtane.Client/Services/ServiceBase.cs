using System;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ServiceBase
    {
        public string CreateApiUrl(string absoluteUri, string serviceName)
        {
            Uri uri = new Uri(absoluteUri);
            string apiurl = uri.Scheme + "://" + uri.Authority + "/";
            string alias = Utilities.GetAlias(absoluteUri);
            if (alias != "")
            {
                apiurl += alias;
            }
            else
            {
                apiurl += "~/";
            }
            apiurl += "api/" + serviceName;
            return apiurl;
        }
    }
}
