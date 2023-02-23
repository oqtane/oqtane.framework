using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and update system information.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// returns a key-value directory with the current system configuration information
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetSystemInfoAsync();

        /// <summary>
        /// returns a key-value directory with the current system information - "environment" or "configuration"
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
    }
}
