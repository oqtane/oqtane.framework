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
        public LocalizationService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Localization");

        public async Task<IEnumerable<Culture>> GetCulturesAsync(bool installed) => await GetJsonAsync<IEnumerable<Culture>>($"{Apiurl}?installed={installed}");
    }
}
