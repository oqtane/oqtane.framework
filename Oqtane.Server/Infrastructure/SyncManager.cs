using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public class SyncManager : ISyncManager
    {
        private List<SyncEvent> SyncEvents { get; set; }

        public event EventHandler<SyncEvent> EntityChanged;

        public SyncManager()
        {
            SyncEvents = new List<SyncEvent>();
        }

        public List<SyncEvent> GetSyncEvents(int tenantId, DateTime lastSyncDate)
        {
            return SyncEvents.Where(item => (item.TenantId == tenantId || item.TenantId == -1) && item.ModifiedOn >= lastSyncDate).ToList();
        }

        public void AddSyncEvent(int tenantId, string entityName, int entityId, string action)
        {
            var syncevent = new SyncEvent { TenantId = tenantId, EntityName = entityName, EntityId = entityId, Action = action, ModifiedOn = DateTime.UtcNow };

            // client actions for PageState management
            if (action == SyncEventActions.Refresh || action == SyncEventActions.Reload)
            {
                // trim sync events 
                SyncEvents.RemoveAll(item => item.ModifiedOn < DateTime.UtcNow.AddHours(-1));

                // add sync event
                SyncEvents.Add(syncevent);
            }

            // raise event
            EntityChanged?.Invoke(this, syncevent);
        }
    }
}
