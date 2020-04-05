using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISyncManager
    {
        List<SyncEvent> GetSyncEvents(DateTime lastSyncDate);
        void AddSyncEvent(string entityName, int entityId);
    }
}
