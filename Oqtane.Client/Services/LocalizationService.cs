using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class LocalizationService : ServiceBase, ILocalizationService
    {
        private readonly SiteState _siteState;

        public LocalizationService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("Localization", _siteState.Alias);

        public async Task<IEnumerable<Culture>> GetCulturesAsync() => await GetJsonAsync<IEnumerable<Culture>>(Apiurl);
    }
}
