using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure.EventSubscribers
{
    public class CacheInvalidationEventSubscriber : IEventSubscriber
    {
        private readonly IMemoryCache _cache;

        public CacheInvalidationEventSubscriber(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void EntityChanged(SyncEvent syncEvent)
        {
            if (syncEvent.EntityName == "Site" && syncEvent.Action == SyncEventActions.Refresh)
            {
                _cache.Remove($"site:{syncEvent.TenantId}:{syncEvent.EntityId}");
            }
        }
    }
}
