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
    /// Service to get / create / modify <see cref="StorageLocation"/> objects.
    /// </summary>
    public interface IStorageLocationService
    {
        /// <summary>
        /// Retrieve storage locations of a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<StorageLocation>> GetStorageLocationsAsync(int siteId);

        /// <summary>
        /// Retrieve the information of one <see cref="StorageLocation"/>
        /// </summary>
        /// <param name="StorageLocationId"></param>
        /// <returns></returns>
        Task<StorageLocation> GetStorageLocationAsync(int StorageLocationId);

        /// <summary>
        /// Retrieve the information of one <see cref="StorageLocation"/>
        /// </summary>
        /// <param name="provider">the storage provider name.</param>
        /// <returns></returns>
        Task<StorageLocation> GetStorageLocationAsync(string provider);

        /// <summary>
        /// Get storage providers available in the system.
        /// </summary>
        /// <returns></returns>
        Task<IList<StorageProvider>> GetProvidersAsync();

        /// <summary>
        /// Get the settings of a <see cref="StorageLocation"/>.
        /// </summary>
        /// <param name="StorageLocationId">Reference to a <see cref="StorageLocation"/></param>
        Task<IDictionary<string, string>> GetSettingsAsync(int StorageLocationId);

        /// <summary>
        /// Get the default <see cref="StorageLocation"/>.
        /// </summary>
        /// <returns></returns>
        Task<int> GetDefaultStorageLocationIdAsync();

        /// <summary>
        /// Create a storage location 
        /// </summary>
        /// <param name="StorageLocation"></param>
        /// <returns></returns>
        Task<StorageLocation> AddStorageLocationAsync(StorageLocation StorageLocation);

        /// <summary>
        /// Update the information about a <see cref="StorageLocation"/>
        /// </summary>
        /// <param name="StorageLocation"></param>
        /// <returns></returns>
        Task<StorageLocation> UpdateStorageLocationAsync(StorageLocation StorageLocation);

        /// <summary>
        /// Update the settings of a <see cref="StorageLocation"/>
        /// </summary>
        /// <param name="StorageLocationId"></param>
        /// <param name="settings"></param>
        Task SaveSettingsAsync(int StorageLocationId, IDictionary<string, string> settings);

        /// <summary>
        /// Delete a <see cref="StorageLocation"/>
        /// </summary>
        /// <param name="StorageLocationId">Reference to a <see cref="StorageLocation"/></param>
        Task DeleteStorageLocationAsync(int StorageLocationId);

    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class StorageLocationService : ServiceBase, IStorageLocationService
    {
        public StorageLocationService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("StorageLocation");

        public async Task<List<StorageLocation>> GetStorageLocationsAsync(int siteId)
        {
            return await GetJsonAsync<List<StorageLocation>>($"{ApiUrl}?siteid={siteId}");
        }

        public async Task<StorageLocation> GetStorageLocationAsync(int StorageLocationId)
        {
            return await GetJsonAsync<StorageLocation>($"{ApiUrl}/id/{StorageLocationId}");
        }

        public async Task<StorageLocation> GetStorageLocationAsync(string provider)
        {
            return await GetJsonAsync<StorageLocation>($"{ApiUrl}/provider/{provider}");
        }

        public async Task<IList<StorageProvider>> GetProvidersAsync()
        {
            return await GetJsonAsync<IList<StorageProvider>>($"{ApiUrl}/providers");
        }

        public async Task<IDictionary<string, string>> GetSettingsAsync(int StorageLocationId)
        {
            return await GetJsonAsync<IDictionary<string, string>>($"{ApiUrl}/settings/{StorageLocationId}");
        }

        public async Task<int> GetDefaultStorageLocationIdAsync()
        {
            return await GetJsonAsync<int>($"{ApiUrl}/default");
        }

        public async Task<StorageLocation> AddStorageLocationAsync(StorageLocation StorageLocation)
        {
            return await PostJsonAsync<StorageLocation>(ApiUrl, StorageLocation);
        }

        public async Task<StorageLocation> UpdateStorageLocationAsync(StorageLocation StorageLocation)
        {
            return await PutJsonAsync<StorageLocation>($"{ApiUrl}/{StorageLocation.StorageLocationId}", StorageLocation);
        }

        public async Task SaveSettingsAsync(int StorageLocationId, IDictionary<string, string> settings)
        {
            await PostJsonAsync($"{ApiUrl}/settings/{StorageLocationId}", settings);
        }

        public async Task DeleteStorageLocationAsync(int StorageLocationId)
        {
            await DeleteAsync($"{ApiUrl}/{StorageLocationId}");
        }
    }
}
