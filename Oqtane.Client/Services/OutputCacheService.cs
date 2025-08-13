using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

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

    /// <inheritdoc cref="IOutputCacheService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class OutputCacheService : ServiceBase, IOutputCacheService
    {
        public OutputCacheService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("OutputCache");

        public async Task EvictByTag(string tag)
        {
            await DeleteAsync($"{ApiUrl}/{tag}");
        }
    }
}
