using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and update system information.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// returns a key-value dictionary with the current system configuration information
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetSystemInfoAsync();

        /// <summary>
        /// returns a key-value dictionary with the current system information - "environment" or "configuration"
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetSystemInfoAsync(string type);

        /// <summary>
        /// returns a config value
        /// </summary>
        /// <returns></returns>
        Task<object> GetSystemInfoAsync(string settingKey, object defaultValue);

        /// <summary>
        /// Updates system information
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task UpdateSystemInfoAsync(Dictionary<string, object> settings);

        /// <summary>
        /// returns a key-value dictionary with default system icons
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetIconsAsync();
    }

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

        public async Task<Dictionary<string, string>> GetIconsAsync()
        {
            return await GetJsonAsync<Dictionary<string, string>>($"{Apiurl}/icons");
        }
    }
}
