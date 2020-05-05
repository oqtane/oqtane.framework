using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISyncManager
    {
        List<SyncEvent> GetSyncEvents(int tenantId, DateTime lastSyncDate);
        void AddSyncEvent(int tenantId, string entityName, int entityId);
    }
}
