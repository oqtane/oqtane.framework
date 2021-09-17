using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve <see cref="Database"/> information.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Returns a list of databases
        /// </summary>
        /// <returns></returns>
        Task<List<Database>> GetDatabasesAsync();
    }
}
