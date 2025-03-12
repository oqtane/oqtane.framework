using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Globalization;

namespace Oqtane.Services
{
    /// <inheritdoc cref="ICookieConsentService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class CookieConsentService : ServiceBase, ICookieConsentService
    {
        public CookieConsentService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("CookieConsent");

        public async Task<bool> IsActionedAsync()
        {
            return await GetJsonAsync<bool>($"{ApiUrl}/IsActioned");
        }

        public async Task<bool> CanTrackAsync(bool optOut)
        {
            return await GetJsonAsync<bool>($"{ApiUrl}/CanTrack?optout=" + optOut);
        }

        public async Task<string> CreateActionedCookieAsync()
        {
            var cookie = await GetStringAsync($"{ApiUrl}/CreateActionedCookie");
            return cookie ?? string.Empty;
        }

        public async Task<string> CreateConsentCookieAsync()
        {
            var cookie = await GetStringAsync($"{ApiUrl}/CreateConsentCookie");
            return cookie ?? string.Empty;
        }

        public async Task<string> WithdrawConsentCookieAsync()
        {
            var cookie = await GetStringAsync($"{ApiUrl}/WithdrawConsentCookie");
            return cookie ?? string.Empty;
        }
    }
}
