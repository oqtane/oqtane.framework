using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    internal class CookieConsentMiddleware
    {
        private readonly IList<string> _defaultEssentialCookies = new List<string>
        {
            ".AspNetCore.Culture",
            "X-XSRF-TOKEN-COOKIE",
            ".AspNetCore.Identity.Application"
        };

        private readonly RequestDelegate _next;

        public CookieConsentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if framework is installed
            var config = context.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
            var settingService = context.RequestServices.GetService(typeof(ISettingService)) as ISettingService;
            var cookieConsentService = context.RequestServices.GetService(typeof(ICookieConsentService)) as ICookieConsentService;
            string path = context.Request.Path.ToString();

            if (config.IsInstalled())
            {
                try
                {
                    var settings = (Dictionary<string, string>)context.Items[Constants.HttpContextSiteSettingsKey];
                    if (settings != null)
                    {
                        var cookieConsentEnabled = bool.Parse(settingService.GetSetting(settings, "CookieConsent", "False"));
                        if (cookieConsentEnabled && !await cookieConsentService.CanTrackAsync())
                        {
                            //only allow essential cookies when consent is not granted
                            var loginCookieName = settingService.GetSetting(settings, "LoginOptions:CookieName", ".AspNetCore.Identity.Application");
                            var cookiesSetting = settingService.GetSetting(settings, "EssentialCookies", string.Empty);

                            var essentialCookies = !string.IsNullOrEmpty(cookiesSetting) ? cookiesSetting.Split(",").ToList() : _defaultEssentialCookies;

                            foreach (var cookie in context.Request.Cookies)
                            {
                                if (cookie.Key != loginCookieName && !essentialCookies.Contains(cookie.Key))
                                {
                                    context.Response.Cookies.Delete(cookie.Key);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }

            // continue processing
            if (_next != null) await _next(context);
        }
    }
}
