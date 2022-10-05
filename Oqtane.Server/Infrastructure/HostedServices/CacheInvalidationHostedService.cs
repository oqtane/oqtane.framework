using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Oqtane.Models;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class CacheInvalidationHostedService : IHostedService
    {
        private readonly ISyncManager _syncManager;
        private readonly IMemoryCache _cache;

        public CacheInvalidationHostedService(ISyncManager syncManager, IMemoryCache cache)
        {
            _syncManager = syncManager;
            _cache = cache;
        }

        void EntityChanged(object sender, SyncEvent e)
        {
            if (e.EntityName == "Site" && e.Action == SyncEventActions.Refresh)
            {
                _cache.Remove($"site:{e.TenantId}:{e.EntityId}");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _syncManager.EntityChanged += EntityChanged;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _syncManager.EntityChanged -= EntityChanged;
        }
    }
}
