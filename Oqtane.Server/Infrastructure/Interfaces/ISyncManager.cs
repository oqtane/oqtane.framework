using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure.Interfaces
{
    public interface ISyncManager
    {
        List<SyncEvent> GetSyncEvents(DateTime lastSyncDate);
        void AddSyncEvent(string entityName, int entityId);
    }
}
