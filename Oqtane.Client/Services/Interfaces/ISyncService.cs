using Oqtane.Models;
using System;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve <see cref="Sync"/> information.
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Get sync events
        /// </summary>
        /// <param name="lastSyncDate"></param>
        /// <returns></returns>
        Task<Sync> GetSyncAsync(DateTime lastSyncDate);
    }
}
