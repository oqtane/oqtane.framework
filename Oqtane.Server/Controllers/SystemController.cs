using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using Microsoft.AspNetCore.Hosting;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SystemController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public SystemController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public Dictionary<string, string> Get()
        {
            Dictionary<string, string> systeminfo = new Dictionary<string, string>();
            systeminfo.Add("clrversion", Environment.Version.ToString());
            systeminfo.Add("osversion", Environment.OSVersion.ToString());
            systeminfo.Add("machinename", Environment.MachineName);
            systeminfo.Add("serverpath", _environment.ContentRootPath);
            systeminfo.Add("servertime", DateTime.Now.ToString());
            return systeminfo;
        }

    }
}
