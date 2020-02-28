using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System.Linq;
using System;
using System.Net;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]

    public class PingController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return "Pong";
        }
    }
}