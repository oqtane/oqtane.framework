using System.Threading;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage cache
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Evicts the output cache for a specific tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task EvictOutputCacheByTag(string tag, CancellationToken cancellationToken = default);
    }
}
