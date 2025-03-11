using System.Threading;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage cache
    /// </summary>
    public interface IOutputCacheService
    {
        /// <summary>
        /// Evicts the output cache for a specific tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task EvictByTag(string tag);
    }
}
