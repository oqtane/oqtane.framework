using System;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ServiceBase
    {

        public static string CreateApiUrl(Alias alias, string absoluteUri, string serviceName)
        {
            Uri uri = new Uri(absoluteUri);

            string apiurl;
            if (alias != null)
            {
                // build a url which passes the alias that may include a subfolder for multi-tenancy
                apiurl = uri.Scheme + "://" + alias.Name + "/";
                if (alias.Path == "")
                {
                    apiurl += "~/";
                }
            }
            else
            {
                // build a url which ignores any subfolder for multi-tenancy
                apiurl = uri.Scheme + "://" + uri.Authority + "/~/";
            }
            apiurl += "api/" + serviceName;
            return apiurl;
        }
    }
}
