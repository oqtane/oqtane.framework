using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DatabaseController : Controller
    {
        private IOptions<List<Models.Database>> _databaseOptions;
        private IConfigManager _config;

        public DatabaseController(IOptions<List<Models.Database>> databaseOptions, IConfigManager config)
        {
            _databaseOptions = databaseOptions;
            _config = config;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Models.Database> Get()
        {
            var databases = _databaseOptions.Value;
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // on non-Windows platforms, LocalDB is not supported, so remove it from the list of available databases
                databases.RemoveAll(item => item.Name == "LocalDB");
            }
            var master = _config.GetSetting(SettingKeys.DatabaseSection, SettingKeys.DatabaseTypeKey, "");
            if (master != "" && databases.Exists(item => item.DBType == master))
            {
                databases.Find(item => item.DBType == master).IsDefault = true;
            }
            return databases;
        }
    }
}
