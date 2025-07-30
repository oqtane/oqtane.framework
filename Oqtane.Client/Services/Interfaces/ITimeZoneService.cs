using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve <see cref="TimeZone"/> entries
    /// </summary>
    public interface ITimeZoneService
    {
        /// <summary>
        /// Get the list of time zones
        /// </summary>
        /// <returns></returns>
        List<TimeZone> GetTimeZones();
    }
}
