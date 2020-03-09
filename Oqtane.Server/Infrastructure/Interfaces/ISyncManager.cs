using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    public interface ISyncManager
    {
        List<SyncEvent> GetSyncEvents(DateTime LastSyncDate);
        void AddSyncEvent(string EntityName, int EntityId);
    }
}
