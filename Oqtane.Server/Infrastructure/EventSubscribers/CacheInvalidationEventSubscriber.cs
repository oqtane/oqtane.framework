using Microsoft.Extensions.Caching.Memory;
using Oqtane.Extensions;
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
            // when site entities change (ie. site, pages, modules, etc...) a site refresh event is raised and the site cache item needs to be refreshed
            if (syncEvent.EntityName == EntityNames.Site && (syncEvent.Action == SyncEventActions.Refresh || syncEvent.Action == SyncEventActions.Reload))
            {
                _cache.Remove($"site:{syncEvent.TenantId}:{syncEvent.EntityId}");
                _cache.Remove($"modules:{syncEvent.TenantId}:{syncEvent.EntityId}");
            }

            // when a site entity is updated, the hosting model may have changed so the client assemblies cache items need to be refreshed
            if (syncEvent.EntityName == EntityNames.Site && (syncEvent.Action == SyncEventActions.Update || syncEvent.Action == SyncEventActions.Delete))
            {
                _cache.Remove($"assemblieslist:{syncEvent.TenantId}:{syncEvent.EntityId}");
                _cache.Remove($"assemblies:{syncEvent.TenantId}:{syncEvent.EntityId}");
            }

            // when a users settings are changed, the user cache item needs to be refreshed
            if (syncEvent.EntityName == EntityNames.User && syncEvent.Action == SyncEventActions.Update)
            {
                _cache.Remove($"user:{syncEvent.EntityId}:{syncEvent.TenantId}:{syncEvent.SiteId}");
            }
        }
    }
}
