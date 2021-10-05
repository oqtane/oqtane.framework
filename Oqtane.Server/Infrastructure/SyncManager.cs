using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class SyncManager : ISyncManager
    {
        private List<SyncEvent> SyncEvents { get; set; }

        public SyncManager()
        {
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
            // trim sync events 
            SyncEvents.RemoveAll(item => item.ModifiedOn < DateTime.UtcNow.AddHours(-1));
        }
    }
}
