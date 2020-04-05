using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly IInstallationManager _installationManager;
        private readonly DatabaseManager _databaseManager;

        public InstallationController(IConfigurationRoot config, IInstallationManager installationManager, DatabaseManager databaseManager)
        {
            _config = config;
            _installationManager = installationManager;
            _databaseManager = databaseManager;
        }

        // POST api/<controller>
        [HttpPost]
        public Installation Post([FromBody] InstallConfig config)
        {
            //TODO Security ????
            var installation = new Installation {Success = false, Message = ""};

            if (ModelState.IsValid && (!_databaseManager.IsInstalled || !config.IsMaster))
            {
                bool master = config.IsMaster;

                config.Alias = config.Alias ?? HttpContext.Request.Host.Value;
                var result = DatabaseManager.InstallDatabase(config);

                if (result.Success)
                {
                    if (master)
                    {
                        _config.Reload();
                    }

                    installation.Success = true;
                    return installation;
                }

                installation.Message = result.Message;
                return installation;
            }

            installation.Message = "Application Is Already Installed";
            return installation;
        }

        // GET api/<controller>/installed
        [HttpGet("installed")]
        public Installation IsInstalled()
        {
            var installation = new Installation {Success = false, Message = ""};

            installation.Success = _databaseManager.IsInstalled;
            installation.Message = _databaseManager.Message;

            return installation;
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
