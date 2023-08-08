using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Globalization;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SyncController : Controller
    {
        private readonly ISyncManager _syncManager;
        private readonly Alias _alias;

        public SyncController(ISyncManager syncManager, ITenantManager tenantManager)
        {
            _syncManager = syncManager;
            _alias = tenantManager.GetAlias();
        }

        // GET api/<controller>/yyyyMMddHHmmssfff
        [HttpGet("{lastSyncDate}")]
        public Sync Get(string lastSyncDate)
        {
            DateTime currentdate = DateTime.UtcNow;
            DateTime lastdate = DateTime.ParseExact(lastSyncDate, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            if (lastdate == DateTime.MinValue)
            {
                lastdate = currentdate;
            }
            Sync sync = new Sync
            {
                SyncDate = currentdate,
                SyncEvents = _syncManager.GetSyncEvents(_alias.TenantId, lastdate)
            };
            return sync;
        }        
    }
}
