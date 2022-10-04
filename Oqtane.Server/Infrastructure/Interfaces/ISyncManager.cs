using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISyncManager
    {
        event EventHandler<SyncEvent> EntityChanged;
        List<SyncEvent> GetSyncEvents(int tenantId, DateTime lastSyncDate);
        void AddSyncEvent(int tenantId, string entityName, int entityId, string action);
    }
}
