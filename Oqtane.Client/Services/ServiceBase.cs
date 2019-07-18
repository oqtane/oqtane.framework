using System;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ServiceBase
    {

        public string CreateApiUrl(Alias alias, string absoluteUri, string serviceName)
        {
            string apiurl = "";
            if (alias != null)
            {
                // build a url which passes the alias that may include a subfolder for multi-tenancy
                apiurl = alias.Url + "/";
                if (alias.Path == "")
                {
                    apiurl += "~/";
                }
            }
            else
            {
                // build a url which ignores any subfolder for multi-tenancy
                Uri uri = new Uri(absoluteUri);
                apiurl = uri.Scheme + "://" + uri.Authority + "/~/";
            }
            apiurl += "api/" + serviceName;
            return apiurl;
        }
    }
}
