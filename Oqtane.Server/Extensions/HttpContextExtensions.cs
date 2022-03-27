using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Extensions
{
    public static class HttpContextExtensions
    {
        public static Alias GetAlias(this HttpContext context)
        {
            if (context != null && context.Items.ContainsKey(Constants.HttpContextAliasKey))
            {
                return context.Items[Constants.HttpContextAliasKey] as Alias;
            }
            return null;
        }

        public static Dictionary<string, string> GetSiteSettings(this HttpContext context)
        {
            if (context != null && context.Items.ContainsKey(Constants.HttpContextSiteSettingsKey))
            {
                return context.Items[Constants.HttpContextSiteSettingsKey] as Dictionary<string, string>;
            }
            return null;
        }
    }
}
