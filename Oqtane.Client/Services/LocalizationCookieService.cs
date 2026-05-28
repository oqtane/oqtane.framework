using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

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
        /// <param name="uiCulture"></param>
        /// <returns></returns>
        Task SetLocalizationCookieAsync(string culture, string uiCulture);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class LocalizationCookieService : ServiceBase, ILocalizationCookieService
    {
        public LocalizationCookieService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        public Task SetLocalizationCookieAsync(string culture, string uiCulture)
        {
            return Task.CompletedTask; // only used in server side rendering
        }
    }
}
