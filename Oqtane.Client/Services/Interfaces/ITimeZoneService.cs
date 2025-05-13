using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retrieve <see cref="TimeZone"/> entries
    /// </summary>
    public interface ITimeZoneService
    {
        /// <summary>
        /// Get the list of time zones
        /// </summary>
        /// <returns></returns>
        Task<List<TimeZone>> GetTimeZonesAsync();
    }
}
