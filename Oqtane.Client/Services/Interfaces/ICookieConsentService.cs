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
        /// Get cookie consent status
        /// </summary>
        /// <returns></returns>
        Task<bool> CanTrackAsync();

        /// <summary>
        /// Grant cookie consent
        /// </summary>
        /// <returns></returns>
        Task<string> CreateConsentCookieAsync();
    }
}
