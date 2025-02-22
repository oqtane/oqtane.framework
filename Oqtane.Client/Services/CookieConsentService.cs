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

        /// <inheritdoc />
        public async Task<bool> CanTrackAsync()
        {
            return await GetJsonAsync<bool>($"{ApiUrl}/CanTrack");
        }

        public async Task<string> CreateConsentCookieAsync()
        {
            var cookie = await GetStringAsync($"{ApiUrl}/CreateConsentCookie");
            return cookie ?? string.Empty;
        }
    }
}
