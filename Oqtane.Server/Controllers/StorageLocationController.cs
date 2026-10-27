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
    public class StorageLocationController : Controller
    {
        private readonly IStorageLocationRepository _StorageLocations;
        private readonly IStorageProviderFactory _storageProviderFactory;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Alias _alias;

        public StorageLocationController(
            IStorageLocationRepository StorageLocations,
            IStorageProviderFactory storageProviderFactory,
            ISyncManager syncManager,
            ILogManager logger,
            IServiceProvider serviceProvider,
            ITenantManager tenantManager)
        {
            _StorageLocations = StorageLocations;
            _storageProviderFactory = storageProviderFactory;
            _syncManager = syncManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<StorageLocation> Get(string siteid)
        {
            var StorageLocations = new List<StorageLocation>();
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                StorageLocations = _StorageLocations.GetStorageLocations(SiteId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                StorageLocations = null;
            }
            return StorageLocations;
        }

        // GET api/<controller>/id/5
        [HttpGet("id/{id}")]
        public StorageLocation GetById(int id)
        {
            var StorageLocation = _StorageLocations.GetStorageLocation(id);
            if (StorageLocation != null && StorageLocation.SiteId.GetValueOrDefault(_alias.SiteId) == _alias.SiteId)
            {
                return StorageLocation;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Get Attempt {StorageLocationId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        // GET api/<controller>/provider/default
        [HttpGet("provider/{provider}")]
        public StorageLocation GetByProvider(string provider)
        {
            var StorageLocation = _StorageLocations.GetStorageLocations(_alias.SiteId).FirstOrDefault(i => i.Provider == provider);
            if (StorageLocation != null)
            {
                return StorageLocation;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Get Attempt {provider}", provider);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        [HttpGet("providers")]
        public IList<StorageProvider> GetProviders()
        {
            return _serviceProvider.GetServices<IStorageProvider>()
                .Select(i => new StorageProvider
                {
                    DisplayName = i.DisplayName,
                    Type = $"{i.GetType().FullName}, {i.GetType().Assembly.GetName().Name}",
                    ControlType = i.SettingControlType
                })
                .OrderBy(i => i.Type switch
                {
                    Constants.PublicStorageProvider => 1,
                    Constants.PrivateStorageProvider => 2,
                    _ => 3
                })
                .ThenBy(i => i.DisplayName)
                .ToList();

        }

        [HttpGet("settings/{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public IDictionary<string, string> GetSettings(int id)
        {
            var StorageLocation = _StorageLocations.GetStorageLocation(id);
            if (StorageLocation != null && StorageLocation.SiteId.GetValueOrDefault(_alias.SiteId) == _alias.SiteId)
            {
                return _StorageLocations.GetSettings(id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Settings Get Attempt {StorageLocationId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return null;
            }
        }

        [HttpGet("default")]
        public int GetDefaultConfigId()
        {
            return _storageProviderFactory.GetDefaultConfigId(_alias.SiteId);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public StorageLocation Post([FromBody] StorageLocation StorageLocation)
        {
            if (ModelState.IsValid && StorageLocation.SiteId == _alias.SiteId)
            {
                if(Constants.DefaultStorageProviders.Contains(StorageLocation.Provider) || !StorageLocation.SiteId.HasValue)
                {
                    throw new ArgumentException("Can't add system default storage location");
                }

                var existing = _StorageLocations.GetStorageLocations(StorageLocation.SiteId.Value)
                    .Any(c => c.Name.Equals(StorageLocation.Name, StringComparison.OrdinalIgnoreCase));
                if(existing)
                {
                    throw new ArgumentException("A storage location with the same name already exists");
                }

                StorageLocation = _StorageLocations.AddStorageLocation(StorageLocation);
                _syncManager.AddSyncEvent(_alias, EntityNames.StorageLocation, StorageLocation.StorageLocationId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Storage Location Added {StorageLocation}", StorageLocation);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Post Attempt {StorageLocation}", StorageLocation);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                StorageLocation = null;
            }
            return StorageLocation;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<StorageLocation> PutAsync(int id, [FromBody] StorageLocation StorageLocation)
        {
            if (ModelState.IsValid && StorageLocation.SiteId == _alias.SiteId && StorageLocation.StorageLocationId == id && _StorageLocations.GetStorageLocation(StorageLocation.StorageLocationId) != null)
            {
                if (Constants.DefaultStorageProviders.Contains(StorageLocation.Provider) || !StorageLocation.SiteId.HasValue)
                {
                    throw new ArgumentException("Default storage location cannot be updated.");
                }

                StorageLocation = _StorageLocations.UpdateStorageLocation(StorageLocation);
                _syncManager.AddSyncEvent(_alias, EntityNames.StorageLocation, StorageLocation.StorageLocationId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Storage Location Updated {StorageLocation}", StorageLocation);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Put Attempt {StorageLocation}", StorageLocation);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                StorageLocation = null;
            }
            return StorageLocation;
        }

        [HttpPost("settings/{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void SaveSettings(int id, [FromBody] IDictionary<string, string> settings)
        {
            var StorageLocation = _StorageLocations.GetStorageLocation(id);
            if (StorageLocation != null && StorageLocation.SiteId == _alias.SiteId)
            {
                _StorageLocations.SaveSettings(id, settings);
                _syncManager.AddSyncEvent(_alias, EntityNames.StorageLocation, StorageLocation.StorageLocationId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Storage Location Settings Saved {Id}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Settings Save Attempt {Id}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task Delete(int id)
        {
            var StorageLocation = _StorageLocations.GetStorageLocation(id);
            if (StorageLocation != null && StorageLocation.SiteId == _alias.SiteId)
            {
                if (Constants.DefaultStorageProviders.Contains(StorageLocation.Provider) || !StorageLocation.SiteId.HasValue)
                {
                    throw new ArgumentException("Default storage location cannot be deleted.");
                }

                _StorageLocations.DeleteStorageLocation(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.StorageLocation, id, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Storage Location Deleted {StorageLocationId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Storage Location Delete Attempt {StorageLocationId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
