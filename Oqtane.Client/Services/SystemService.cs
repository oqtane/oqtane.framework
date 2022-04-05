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

        public async Task<Dictionary<string, object>> GetSystemInfoAsync()
        {
            return await GetSystemInfoAsync("configuration");
        }

        public async Task<Dictionary<string, object>> GetSystemInfoAsync(string type)
        {
            return await GetJsonAsync<Dictionary<string, object>>($"{Apiurl}?type={type}");
        }

        public async Task<object> GetSystemInfoAsync(string settingKey, object defaultValue)
        {
            return await GetJsonAsync<object>($"{Apiurl}/{settingKey}/{defaultValue}");
        }

        public async Task UpdateSystemInfoAsync(Dictionary<string, object> settings)
        {
            await PostJsonAsync(Apiurl, settings);
        }
        public async Task UpdateSystemInfoAsync(string settingKey, object settingValue)
        {
            await PutJsonAsync($"{Apiurl}/{settingKey}/{settingValue}", "");
        }
    }
}
