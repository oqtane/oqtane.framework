using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure.EventSubscribers
{
    public class CacheInvalidationEventSubscriber : IEventSubscriber
    {
        private readonly ICacheManager _cache;

        public CacheInvalidationEventSubscriber(ICacheManager cache)
        {
            _cache = cache;
        }

        public void EntityChanged(SyncEvent syncEvent)
        {
            // when site entities change (ie. site, pages, modules, etc...) a site refresh event is raised and the site cache item needs to be refreshed
            if (syncEvent.EntityName == EntityNames.Site && (syncEvent.Action == SyncEventActions.Refresh || syncEvent.Action == SyncEventActions.Reload))
            {
                _cache.RemoveCache($"SiteKey:{syncEvent.SiteKey}:Site");
                _cache.RemoveCache($"SiteKey:{syncEvent.SiteKey}:Modules");
            }

            // when a site entity is updated, the hosting model may have changed so the client assemblies cache items need to be refreshed
            if (syncEvent.EntityName == EntityNames.Site && (syncEvent.Action == SyncEventActions.Update || syncEvent.Action == SyncEventActions.Delete))
            {
                _cache.RemoveCache($"SiteKey:{syncEvent.SiteKey}:AssembliesList");
                _cache.RemoveCache($"SiteKey:{syncEvent.SiteKey}:Assemblies");
            }

            // when a users settings are changed, the user cache item needs to be refreshed
            if (syncEvent.EntityName == EntityNames.User && syncEvent.Action == SyncEventActions.Update)
            {
                _cache.RemoveCache($"SiteKey:{syncEvent.SiteKey}:User:{syncEvent.EntityId}");
            }
        }
    }
}
