using Oqtane.Models;
using System;
using System.Threading.Tasks;

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
}
