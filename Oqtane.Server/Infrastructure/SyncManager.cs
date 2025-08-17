using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public interface ISyncManager
    {
        event EventHandler<SyncEvent> EntityChanged;
        List<SyncEvent> GetSyncEvents(int tenantId, DateTime lastSyncDate);
        void AddSyncEvent(Alias alias, string entityName, int entityId, string action);
        void AddSyncEvent(int tenantId, int siteId, string entityName, int entityId, string action);

        [Obsolete("AddSyncEvent(int tenantId, string entityName, int entityId, string action) is deprecated. Use AddSyncEvent(Alias alias, string entityName, int entityId, string action) instead.", false)]
        void AddSyncEvent(int tenantId, string entityName, int entityId, string action);
    }

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

        public void AddSyncEvent(Alias alias, string entityName, int entityId, string action)
        {
            AddSyncEvent(alias.TenantId, alias.SiteId, entityName, entityId, action);
        }

        public void AddSyncEvent(int tenantId, int siteId, string entityName, int entityId, string action)
        {
            var syncevent = new SyncEvent { TenantId = tenantId, SiteId = siteId, EntityName = entityName, EntityId = entityId, Action = action, ModifiedOn = DateTime.UtcNow };

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


        // deprecated
        public void AddSyncEvent(int tenantId, string entityName, int entityId, string action)
        {
            AddSyncEvent(tenantId, -1, entityName, entityId, action);
        }
    }
}
