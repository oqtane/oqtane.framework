using System;
using System.Collections.Generic;
using Oqtane.Models;

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
}
