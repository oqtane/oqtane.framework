using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Providers;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class FolderConfigController : Controller
    {
        private readonly IFolderConfigRepository _folderConfigs;
        private readonly IFolderProviderFactory _folderProviderFactory;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Alias _alias;

        public FolderConfigController(
            IFolderConfigRepository folderConfigs,
            IFolderProviderFactory folderProviderFactory,
            IUserPermissions userPermissions,
            ISyncManager syncManager,
            ILogManager logger,
            IServiceProvider serviceProvider,
            ITenantManager tenantManager)
        {
            _folderConfigs = folderConfigs;
            _folderProviderFactory = folderProviderFactory;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<FolderConfig> Get(string siteid)
        {
            var folderConfigs = new List<FolderConfig>();
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                folderConfigs = _folderConfigs.GetFolderConfigs(SiteId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folderConfigs = null;
            }
            return folderConfigs;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public FolderConfig Get(int id)
        {
            var folderConfig = _folderConfigs.GetFolderConfig(id);
            if (folderConfig != null && folderConfig.SiteId == _alias.SiteId)
            {
                return folderConfig;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Get Attempt {FolderConfigId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        [HttpGet("providers")]
        public IDictionary<string, string> GetProviders()
        {
            return _serviceProvider.GetServices<IFolderProvider>()
                .ToDictionary(i => i.Name, i => i.DisplayName);
        }

        [HttpGet("settingtype/{provider}")]
        public string GetSettingType(string provider)
        {
            if (!string.IsNullOrEmpty(provider))
            {
                return _serviceProvider.GetServices<IFolderProvider>()
                    .FirstOrDefault(i => i.Name.Equals(provider, StringComparison.OrdinalIgnoreCase))
                    ?.SettingType;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Get Attempt {Provider}", provider);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        [HttpGet("settings/{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public IDictionary<string, string> GetSettings(int id)
        {
            var folderConfig = _folderConfigs.GetFolderConfig(id);
            if (folderConfig != null && folderConfig.SiteId == _alias.SiteId)
            {
                return _folderConfigs.GetSettings(id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Settings Get Attempt {FolderConfigId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        [HttpGet("default")]
        public int GetDefaultConfigId()
        {
            return _folderProviderFactory.GetDefaultConfigId(_alias.SiteId);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public FolderConfig Post([FromBody] FolderConfig folderConfig)
        {
            if (ModelState.IsValid && folderConfig.SiteId == _alias.SiteId)
            {
                if(folderConfig.Provider == Constants.DefaultFolderProvider)
                {
                    throw new ArgumentException("Default folder provider cannot be added as a folder config.");
                }

                var exsiting = _folderConfigs.GetFolderConfigs(folderConfig.SiteId)
                    .Any(fpc => fpc.Name.Equals(folderConfig.Name, StringComparison.OrdinalIgnoreCase));
                if(exsiting)
                {
                    throw new ArgumentException("A folder config with the same name already exists.");
                }

                folderConfig = _folderConfigs.AddFolderConfig(folderConfig);
                _syncManager.AddSyncEvent(_alias, EntityNames.FolderConfig, folderConfig.FolderConfigId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Config Added {FolderConfig}", folderConfig);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Post Attempt {FolderConfig}", folderConfig);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folderConfig = null;
            }
            return folderConfig;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<FolderConfig> PutAsync(int id, [FromBody] FolderConfig folderConfig)
        {
            if (ModelState.IsValid && folderConfig.SiteId == _alias.SiteId && folderConfig.FolderConfigId == id && _folderConfigs.GetFolderConfig(folderConfig.FolderConfigId) != null)
            {
                if (folderConfig.Provider == Constants.DefaultFolderProvider)
                {
                    throw new ArgumentException("Default folder provider cannot be updated.");
                }

                folderConfig = _folderConfigs.UpdateFolderConfig(folderConfig);
                _syncManager.AddSyncEvent(_alias, EntityNames.FolderConfig, folderConfig.FolderConfigId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Config Updated {FolderConfig}", folderConfig);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Put Attempt  {FolderConfig}", folderConfig);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folderConfig = null;
            }
            return folderConfig;
        }

        [HttpPost("settings/{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void SaveSettings(int id, [FromBody] IDictionary<string, string> settings)
        {
            var folderConfig = _folderConfigs.GetFolderConfig(id);
            if (folderConfig != null && folderConfig.SiteId == _alias.SiteId)
            {
                _folderConfigs.SaveSettings(id, settings);
                _syncManager.AddSyncEvent(_alias, EntityNames.FolderConfig, folderConfig.FolderConfigId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Config Settings Saved {Id}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Settings Save Attempt {Id}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task Delete(int id)
        {
            var folderConfig = _folderConfigs.GetFolderConfig(id);
            if (folderConfig != null && folderConfig.SiteId == _alias.SiteId)
            {
                if (folderConfig.Provider == Constants.DefaultFolderProvider)
                {
                    throw new ArgumentException("Default folder provider cannot be deleted.");
                }

                _folderConfigs.DeleteFolderConfig(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.FolderConfig, id, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Folder Config Deleted {FolderConfigId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Config Delete Attempt {FolderConfigId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
