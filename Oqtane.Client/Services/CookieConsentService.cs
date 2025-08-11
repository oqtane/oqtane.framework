using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve cookie consent information.
    /// </summary>
    public interface ICookieConsentService
    {
        /// <summary>
        /// Get cookie consent bar actioned status
        /// </summary>
        /// <returns></returns>
        Task<bool> IsActionedAsync();

        /// <summary>
        /// Get cookie consent status
        /// </summary>
        /// <returns></returns>
        Task<bool> CanTrackAsync(bool optOut);

        /// <summary>
        /// create actioned cookie
        /// </summary>
        /// <returns></returns>
        Task<string> CreateActionedCookieAsync();

        /// <summary>
        /// create consent cookie
        /// </summary>
        /// <returns></returns>
        Task<string> CreateConsentCookieAsync();

        /// <summary>
        /// widhdraw consent cookie
        /// </summary>
        /// <returns></returns>
        Task<string> WithdrawConsentCookieAsync();
    }

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
