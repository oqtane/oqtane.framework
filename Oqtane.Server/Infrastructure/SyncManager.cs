using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public class SyncManager : ISyncManager
    {
        private readonly IServiceScopeFactory ServiceScopeFactory;
        private List<SyncEvent> SyncEvents { get; set; }

        public SyncManager(IServiceScopeFactory ServiceScopeFactory)
        {
            this.ServiceScopeFactory = ServiceScopeFactory;
            SyncEvents = new List<SyncEvent>();
        }

        private int TenantId
        {
            get 
            {
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    return scope.ServiceProvider.GetRequiredService<ITenantResolver>().GetTenant().TenantId;
                }
            }
        }

        public List<SyncEvent> GetSyncEvents(DateTime LastSyncDate)
        {
            return SyncEvents.Where(item => item.TenantId == TenantId && item.ModifiedOn >= LastSyncDate).ToList();
        }

        public void AddSyncEvent(string EntityName, int EntityId)
        {
            SyncEvents.Add(new SyncEvent { TenantId = TenantId, EntityName = EntityName, EntityId = EntityId, ModifiedOn = DateTime.Now });
            // trim sync events 
            SyncEvents.RemoveAll(item => item.ModifiedOn < DateTime.Now.AddHours(-1));
        }
    }
}
