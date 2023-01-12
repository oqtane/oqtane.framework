using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve localizations (<see cref="Culture"/>)
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Returns a collection of supported cultures
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Culture>> GetCulturesAsync(bool installed);
    }
}
