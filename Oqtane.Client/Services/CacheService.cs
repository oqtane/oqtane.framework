using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <inheritdoc cref="ICacheService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class CacheService : ServiceBase, ICacheService
    {
        public CacheService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Cache");

        public async Task EvictOutputCacheByTag(string tag, CancellationToken cancellationToken = default)
        {
            await DeleteAsync($"{ApiUrl}/outputCache/evictByTag/{tag}");
        }
    }
}
