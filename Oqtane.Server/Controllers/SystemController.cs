using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SystemController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;

        public SystemController(IWebHostEnvironment environment, IConfigManager configManager)
        {
            _environment = environment;
            _configManager = configManager;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public Dictionary<string, string> Get()
        {
            Dictionary<string, string> systeminfo = new Dictionary<string, string>();

            systeminfo.Add("rendermode", _configManager.GetSetting("RenderMode", "Server"));
            systeminfo.Add("clrversion", Environment.Version.ToString());
            systeminfo.Add("osversion", Environment.OSVersion.ToString());
            systeminfo.Add("machinename", Environment.MachineName);
            systeminfo.Add("serverpath", _environment.ContentRootPath);
            systeminfo.Add("servertime", DateTime.Now.ToString());

            return systeminfo;
        }

    }
}
