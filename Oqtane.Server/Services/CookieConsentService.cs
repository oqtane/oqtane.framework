using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ServerCookieConsentService : ICookieConsentService
    {
        private readonly IHttpContextAccessor _accessor;

        public ServerCookieConsentService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task<bool> CanTrackAsync()
        {
            var consentFeature = _accessor.HttpContext?.Features.Get<ITrackingConsentFeature>();
            var canTrack = consentFeature?.CanTrack ?? true;

            return Task.FromResult(canTrack);
        }

        public Task<string> CreateConsentCookieAsync()
        {
            var consentFeature = _accessor.HttpContext?.Features.Get<ITrackingConsentFeature>();
            consentFeature?.GrantConsent();
            var cookie = consentFeature?.CreateConsentCookie() ?? string.Empty;

            return Task.FromResult(cookie);
        }
    }
}
