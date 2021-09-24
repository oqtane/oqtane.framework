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
        /// returns a key-value directory with the current system information (os-version, clr-version, etc.)
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetSystemInfoAsync();

        /// <summary>
        /// Updates system information
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task UpdateSystemInfoAsync(Dictionary<string, string> settings);
    }
}
