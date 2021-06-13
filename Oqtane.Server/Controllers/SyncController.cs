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
            Sync sync = new Sync
            {
                SyncDate = DateTime.UtcNow,
                SyncEvents = _syncManager.GetSyncEvents(_alias.TenantId, DateTime.ParseExact(lastSyncDate, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture))
            };
            return sync;
        }        
    }
}
