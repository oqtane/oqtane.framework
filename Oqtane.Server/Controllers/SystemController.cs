using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;
using Microsoft.AspNetCore.Http.Features;

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

        // GET: api/<controller>?type=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public Dictionary<string, object> Get(string type)
        {
            Dictionary<string, object> systeminfo = new Dictionary<string, object>();

            switch (type.ToLower())
            {
                case "environment":
                    systeminfo.Add("CLRVersion", Environment.Version.ToString());
                    systeminfo.Add("OSVersion", Environment.OSVersion.ToString());
                    systeminfo.Add("MachineName", Environment.MachineName);
                    systeminfo.Add("WorkingSet", Environment.WorkingSet.ToString());
                    systeminfo.Add("TickCount", Environment.TickCount64.ToString());
                    systeminfo.Add("ContentRootPath", _environment.ContentRootPath);
                    systeminfo.Add("WebRootPath", _environment.WebRootPath);
                    systeminfo.Add("ServerTime", DateTime.UtcNow.ToString());
                    var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
                    systeminfo.Add("IPAddress", feature?.LocalIpAddress?.ToString());
                    break;
                case "configuration":
                    systeminfo.Add("InstallationId", _configManager.GetInstallationId());
                    systeminfo.Add("Runtime", _configManager.GetSetting("Runtime", "Server"));
                    systeminfo.Add("RenderMode", _configManager.GetSetting("RenderMode", "ServerPrerendered"));
                    systeminfo.Add("DetailedErrors", _configManager.GetSetting("DetailedErrors", "false"));
                    systeminfo.Add("Logging:LogLevel:Default", _configManager.GetSetting("Logging:LogLevel:Default", "Information"));
                    systeminfo.Add("Logging:LogLevel:Notify", _configManager.GetSetting("Logging:LogLevel:Notify", "Error"));
                    systeminfo.Add("UseSwagger", _configManager.GetSetting("UseSwagger", "true"));
                    systeminfo.Add("PackageService", _configManager.GetSetting("PackageService", "true"));
                    break;
            }

            return systeminfo;
        }


        // GET: api/<controller>
        [HttpGet("{key}/{value}")]
        [Authorize(Roles = RoleNames.Host)]
        public object Get(string key, object value)
        {
            return _configManager.GetSetting(key, value);
        }

        // POST: api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public void Post([FromBody] Dictionary<string, object> settings)
        {
            foreach(KeyValuePair<string, object> kvp in settings)
            {
                _configManager.AddOrUpdateSetting(kvp.Key, kvp.Value, false);
            }
        }

        // PUT: api/<controller>
        [HttpPut("{key}/{value}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Put(string key, object value)
        {
            _configManager.AddOrUpdateSetting(key, value, false);
        }
    }
}
