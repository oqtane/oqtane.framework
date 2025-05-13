using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class TimeZoneController : Controller
    {
        public TimeZoneController() {}

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Models.TimeZone> Get()
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Select(item => new Models.TimeZone
                {
                    Id = item.Id,
                    DisplayName = item.DisplayName
                })
                .OrderBy(item => item.DisplayName);
        }
    }
}
