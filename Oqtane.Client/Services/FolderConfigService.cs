using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to get / create / modify <see cref="FolderConfig"/> objects.
    /// </summary>
    public interface IFolderConfigService
    {
        /// <summary>
        /// Retrieve folder config of a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<FolderConfig>> GetFolderConfigsAsync(int siteId);

        /// <summary>
        /// Retrieve the information of one <see cref="FolderConfig"/>
        /// </summary>
        /// <param name="folderConfigId"></param>
        /// <returns></returns>
        Task<FolderConfig> GetFolderConfigAsync(int folderConfigId);

        /// <summary>
        /// Get folder providers available in the system.
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetProvidersAsync();

        /// <summary>
        /// Get the setting type of a <see cref="FolderConfig"/>.
        /// </summary>
        /// <param name="provider">The folder provider name.</param>
        /// <returns></returns>
        Task<string> GetProviderSettingTypeAsync(string provider);

        /// <summary>
        /// Get the settings of a <see cref="FolderConfig"/>.
        /// </summary>
        /// <param name="folderConfigId">Reference to a <see cref="FolderConfig"/></param>
        Task<IDictionary<string, string>> GetSettingsAsync(int folderConfigId);

        /// <summary>
        /// Get the default <see cref="FolderConfig"/>.
        /// </summary>
        /// <returns></returns>
        Task<int> GetDefaultConfigIdAsync();

        /// <summary>
        /// Create one folder config using a <see cref="FolderConfig"/> object. 
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <returns></returns>
        Task<FolderConfig> AddFolderConfigAsync(FolderConfig folderConfig);

        /// <summary>
        /// Update the information about a <see cref="FolderConfig"/>
        /// Use this to rename the folder config etc.
        /// </summary>
        /// <param name="folderConfig"></param>
        /// <returns></returns>
        Task<FolderConfig> UpdateFolderConfigAsync(FolderConfig folderConfig);

        /// <summary>
        /// Update the settings of a <see cref="FolderConfig"/>
        /// </summary>
        /// <param name="folderConfigId"></param>
        /// <param name="settings"></param>
        Task SaveSettingsAsync(int folderConfigId, IDictionary<string, string> settings);

        /// <summary>
        /// Delete a <see cref="FolderConfig"/>
        /// </summary>
        /// <param name="folderConfigId">Reference to a <see cref="FolderConfig"/></param>
        Task DeleteFolderConfigAsync(int folderConfigId);

    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class FolderConfigService : ServiceBase, IFolderConfigService
    {
        public FolderConfigService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("FolderConfig");

        public async Task<List<FolderConfig>> GetFolderConfigsAsync(int siteId)
        {
            return await GetJsonAsync<List<FolderConfig>>($"{ApiUrl}?siteid={siteId}");
        }

        public async Task<FolderConfig> GetFolderConfigAsync(int folderConfigId)
        {
            return await GetJsonAsync<FolderConfig>($"{ApiUrl}/{folderConfigId}");
        }

        public async Task<IDictionary<string, string>> GetProvidersAsync()
        {
            return await GetJsonAsync<IDictionary<string, string>>($"{ApiUrl}/providers");
        }

        public async Task<string> GetProviderSettingTypeAsync(string provider)
        {
            return await GetStringAsync($"{ApiUrl}/settingtype/{provider}");
        }

        public async Task<IDictionary<string, string>> GetSettingsAsync(int folderConfigId)
        {
            return await GetJsonAsync<IDictionary<string, string>>($"{ApiUrl}/settings/{folderConfigId}");
        }

        public async Task<int> GetDefaultConfigIdAsync()
        {
            return await GetJsonAsync<int>($"{ApiUrl}/default");
        }

        public async Task<FolderConfig> AddFolderConfigAsync(FolderConfig folderConfig)
        {
            return await PostJsonAsync<FolderConfig>(ApiUrl, folderConfig);
        }

        public async Task<FolderConfig> UpdateFolderConfigAsync(FolderConfig folderConfig)
        {
            return await PutJsonAsync<FolderConfig>($"{ApiUrl}/{folderConfig.FolderConfigId}", folderConfig);
        }

        public async Task SaveSettingsAsync(int folderConfigId, IDictionary<string, string> settings)
        {
            await PostJsonAsync($"{ApiUrl}/settings/{folderConfigId}", settings);
        }

        public async Task DeleteFolderConfigAsync(int folderConfigId)
        {
            await DeleteAsync($"{ApiUrl}/{folderConfigId}");
        }
    }
}
