using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Models;
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

        public async Task<Culture> GetDefaultCulture() => await GetJsonAsync<Culture>($"{Apiurl}/getDefaultCulture");

        public async Task<IEnumerable<Culture>> GetSupportedCultures()
            => await GetJsonAsync<IEnumerable<Culture>>($"{Apiurl}/getSupportedCultures");
    }
}
