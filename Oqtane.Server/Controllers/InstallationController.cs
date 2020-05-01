using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly IInstallationManager _installationManager;
        private readonly IDatabaseManager _databaseManager;

        public InstallationController(IConfigurationRoot config, IInstallationManager installationManager, IDatabaseManager databaseManager)
        {
            _config = config;
            _installationManager = installationManager;
            _databaseManager = databaseManager;
        }

        // POST api/<controller>
        [HttpPost]
        public Installation Post([FromBody] InstallConfig config)
        {
            var installation = new Installation {Success = false, Message = ""};

            if (ModelState.IsValid && (User.IsInRole(Constants.HostRole) || string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.ConnectionStringKey))))
            {
                installation = _databaseManager.Install(config);
            }
            else
            {
                installation.Message = "Installation Not Authorized";
            }

            return installation;
        }

        // GET api/<controller>/installed
        [HttpGet("installed")]
        public Installation IsInstalled()
        {
            bool isInstalled = _databaseManager.IsInstalled();
            return new Installation {Success = isInstalled, Message = string.Empty};
        }

        [HttpGet("upgrade")]
        [Authorize(Roles = Constants.HostRole)]
        public Installation Upgrade()
        {
            var installation = new Installation {Success = true, Message = ""};
            _installationManager.UpgradeFramework();
            return installation;
        }
    }
}
