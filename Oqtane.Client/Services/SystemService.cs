using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Net;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SystemService : ServiceBase, ISystemService
    {
        public SystemService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("System");

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
    }
}
