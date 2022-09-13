using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oqtane.Infrastructure
{
    public class SyncManager : ISyncManager
    {
        private readonly IMemoryCache _cache;
        private List<SyncEvent> SyncEvents { get; set; }

        public SyncManager(IMemoryCache cache)
        {
            _cache = cache;
            SyncEvents = new List<SyncEvent>();
        }

        public List<SyncEvent> GetSyncEvents(int tenantId, DateTime lastSyncDate)
        {
            return SyncEvents.Where(item => (item.TenantId == tenantId || item.TenantId == -1) && item.ModifiedOn >= lastSyncDate).ToList();
        }

        public void AddSyncEvent(int tenantId, string entityName, int entityId)
        {
            AddSyncEvent(tenantId, entityName, entityId, false);
        }

        public void AddSyncEvent(int tenantId, string entityName, int entityId, bool reload)
        {
            SyncEvents.Add(new SyncEvent { TenantId = tenantId, EntityName = entityName, EntityId = entityId, Reload = reload, ModifiedOn = DateTime.UtcNow });
            if (entityName == EntityNames.Site)
{
                _cache.Remove($"site:{tenantId}:{entityId}");
            }
            // trim sync events 
            SyncEvents.RemoveAll(item => item.ModifiedOn < DateTime.UtcNow.AddHours(-1));
        }
    }
}
