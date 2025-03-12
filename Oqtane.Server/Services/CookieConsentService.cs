using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ServerCookieConsentService : ICookieConsentService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly CookiePolicyOptions _cookiePolicyOptions;

        public ServerCookieConsentService(IHttpContextAccessor accessor, IOptions<CookiePolicyOptions> cookiePolicyOptions)
        {
            _accessor = accessor;
            _cookiePolicyOptions = cookiePolicyOptions.Value;
        }

        public Task<bool> IsActionedAsync()
        {
            var actioned = false;
            if (_accessor.HttpContext != null)
            {
                var cookieValue = GetCookieValue("actioned");
                actioned = cookieValue == Constants.CookieConsentActionCookieValue;
            }
            return Task.FromResult(actioned);
        }

        public Task<bool> CanTrackAsync(bool optOut)
        {
            var canTrack = true;
            if (_accessor.HttpContext != null)
            {
                var cookieValue = GetCookieValue("consent");
                var saved = cookieValue == Constants.CookieConsentCookieValue;
                if (optOut)
                {
                    canTrack = string.IsNullOrEmpty(cookieValue) || !saved;
                }
                else
                {
                    canTrack = cookieValue == Constants.CookieConsentCookieValue;
                }
            }

            return Task.FromResult(canTrack);
        }

        public Task<string> CreateActionedCookieAsync()
        {
            var cookieString = CreateCookieString(false, string.Empty);
            return Task.FromResult(cookieString);
        }

        public Task<string> CreateConsentCookieAsync()
        {
            var cookieString = CreateCookieString(true, Constants.CookieConsentCookieValue);
            return Task.FromResult(cookieString);
        }

        public Task<string> WithdrawConsentCookieAsync()
        {
            var cookieString = CreateCookieString(true, string.Empty);
            return Task.FromResult(cookieString);
        }

        private string GetCookieValue(string type)
        {
            var cookieValue = string.Empty;
            if (_accessor.HttpContext != null)
            {
                var value = _accessor.HttpContext.Request.Cookies[Constants.CookieConsentCookieName];
                var index = type == "actioned" ? 1 : 0;
                cookieValue = !string.IsNullOrEmpty(value) && value.Contains("|") ? value.Split('|')[index] : string.Empty;
            }

            return cookieValue;
        }

        private string CreateCookieString(bool saved, string savedValue)
        {
            var cookieString = string.Empty;
            if (_accessor.HttpContext != null)
            {
                var savedCookie = saved ? savedValue : GetCookieValue("consent");
                var actionedCookie = Constants.CookieConsentActionCookieValue;
                var cookieValue = $"{savedCookie}|{actionedCookie}";
                var options = _cookiePolicyOptions.ConsentCookie.Build(_accessor.HttpContext);

                if (!_accessor.HttpContext.Response.HasStarted)
                {
                    _accessor.HttpContext.Response.Cookies.Append(
                        Constants.CookieConsentCookieName,
                        cookieValue,
                        new CookieOptions()
                        {
                            Expires = options.Expires,
                            IsEssential = true,
                            SameSite = options.SameSite,
                            Secure = options.Secure
                        }
                    );
                }

                //get the cookie string from response header
                cookieString = options.CreateCookieHeader(Constants.CookieConsentCookieName, Uri.EscapeDataString(cookieValue)).ToString();
            }

            return cookieString;
        }
    }
}
