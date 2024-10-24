using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to set localization cookie
    /// </summary>
    public interface ILocalizationCookieService
    {
        /// <summary>
        /// Set the localization cookie
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        Task SetLocalizationCookieAsync(string culture);
    }
}
