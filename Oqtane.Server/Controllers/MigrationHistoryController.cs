using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MigrationHistoryController : Controller
    {
        private readonly IMigrationHistoryRepository _history;

        public MigrationHistoryController(IMigrationHistoryRepository history)
        {
            _history = history;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<MigrationHistory> Get()
        {
            return _history.GetMigrationHistory();
        }
    }
}
