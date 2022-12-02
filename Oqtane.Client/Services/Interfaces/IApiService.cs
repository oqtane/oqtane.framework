using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and update API information.
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// returns a list of APIs
        /// </summary>
        /// <returns></returns>
        Task<List<Api>> GetApisAsync(int siteId);

        /// <summary>
        /// returns a specific API
        /// </summary>
        /// <returns></returns>
        Task<Api> GetApiAsync(int siteId, string entityName);

        /// <summary>
        /// Updates an API
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        Task UpdateApiAsync(Api api);
    }
}
