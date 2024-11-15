using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class LocalizationCookieService : ServiceBase, ILocalizationCookieService
    {
        public LocalizationCookieService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        public Task SetLocalizationCookieAsync(string culture)
        {
            return Task.CompletedTask; // only used in server side rendering
        }
    }
}
