using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DatabaseController : Controller
    {
        private IOptions<List<Database>> _databaseOptions;

        public DatabaseController(IOptions<List<Database>> databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Models.Database> Get()
        {
            return _databaseOptions.Value;
        }
    }
}
