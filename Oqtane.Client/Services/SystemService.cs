using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SystemService : ServiceBase, ISystemService
    {
        private readonly SiteState _siteState;

        public SystemService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("System", _siteState.Alias);

        public async Task<Dictionary<string, string>> GetSystemInfoAsync()
        {
            return await GetJsonAsync<Dictionary<string, string>>(Apiurl);
        }

        public async Task UpdateSystemInfoAsync(Dictionary<string, string> settings)
        {
            await PostJsonAsync(Apiurl, settings);
        }
    }
}
