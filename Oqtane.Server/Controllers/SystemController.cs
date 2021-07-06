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

            systeminfo.Add("clrversion", Environment.Version.ToString());
            systeminfo.Add("osversion", Environment.OSVersion.ToString());
            systeminfo.Add("machinename", Environment.MachineName);
            systeminfo.Add("serverpath", _environment.ContentRootPath);
            systeminfo.Add("servertime", DateTime.Now.ToString());
            systeminfo.Add("installationid", _configManager.GetInstallationId());

            systeminfo.Add("runtime", _configManager.GetSetting("Runtime", "Server"));
            systeminfo.Add("rendermode", _configManager.GetSetting("RenderMode", "ServerPrerendered"));
            systeminfo.Add("detailederrors", _configManager.GetSetting("DetailedErrors", "false"));
            systeminfo.Add("logginglevel", _configManager.GetSetting("Logging:LogLevel:Default", "Information"));
            systeminfo.Add("swagger", _configManager.GetSetting("UseSwagger", "true"));
            systeminfo.Add("packageservice", _configManager.GetSetting("PackageService", "true"));

            return systeminfo;
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public void Post([FromBody] Dictionary<string, string> settings)
        {
            foreach(KeyValuePair<string, string> kvp in settings)
            {
                switch (kvp.Key)
                {
                    case "runtime":
                        _configManager.AddOrUpdateSetting("Runtime", kvp.Value, false);
                        break;
                    case "rendermode":
                        _configManager.AddOrUpdateSetting("RenderMode", kvp.Value, false);
                        break;
                    case "detailederrors":
                        _configManager.AddOrUpdateSetting("DetailedErrors", kvp.Value, false);
                        break;
                    case "logginglevel":
                        _configManager.AddOrUpdateSetting("Logging:LogLevel:Default", kvp.Value, false);
                        break;
                    case "swagger":
                        _configManager.AddOrUpdateSetting("UseSwagger", kvp.Value, false);
                        break;
                    case "packageservice":
                        _configManager.AddOrUpdateSetting("PackageService", kvp.Value, false);
                        break;
                }
            }
        }
    }
}
