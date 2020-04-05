using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class SyncManager : ISyncManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private List<SyncEvent> SyncEvents { get; set; }

        public SyncManager(IServiceScopeFactory serviceScopeFactory)
        {
            this._serviceScopeFactory = serviceScopeFactory;
            SyncEvents = new List<SyncEvent>();
        }

        private int TenantId
        {
            get 
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    return scope.ServiceProvider.GetRequiredService<ITenantResolver>().GetTenant().TenantId;
                }
            }
        }

        public List<SyncEvent> GetSyncEvents(DateTime lastSyncDate)
        {
            return SyncEvents.Where(item => item.TenantId == TenantId && item.ModifiedOn >= lastSyncDate).ToList();
        }

        public void AddSyncEvent(string entityName, int entityId)
        {
            SyncEvents.Add(new SyncEvent { TenantId = TenantId, EntityName = entityName, EntityId = entityId, ModifiedOn = DateTime.UtcNow });
            // trim sync events 
            SyncEvents.RemoveAll(item => item.ModifiedOn < DateTime.UtcNow.AddHours(-1));
        }
    }
}
