using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class LocalizationService : ServiceBase, ILocalizationService
    {
        private readonly SiteState _siteState;

        public LocalizationService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Localization");

        public async Task<string> GetDefaultCulture() => await GetJsonAsync<string>($"{Apiurl}/getDefaultCulture");

        public async Task<string[]> GetSupportedCultures() => await GetJsonAsync<string[]>($"{Apiurl}/getSupportedCultures");
    }
}
