using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.Application.Repository;

namespace Oqtane.Application.Services
{
    public class ServerMyModuleService : IMyModuleService
    {
        private readonly IMyModuleRepository _MyModuleRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerMyModuleService(IMyModuleRepository MyModuleRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _MyModuleRepository = MyModuleRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.MyModule>> GetMyModulesAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_MyModuleRepository.GetMyModules(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.MyModule> GetMyModuleAsync(int MyModuleId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_MyModuleRepository.GetMyModule(MyModuleId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Get Attempt {TaskId} {ModuleId}", MyModuleId, ModuleId);
                return null;
            }
        }

        public Task<Models.MyModule> AddMyModuleAsync(Models.MyModule MyModule)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, MyModule.ModuleId, PermissionNames.Edit))
            {
                MyModule = _MyModuleRepository.AddMyModule(MyModule);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "MyModule Added {MyModule}", MyModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Add Attempt {MyModule}", MyModule);
                MyModule = null;
            }
            return Task.FromResult(MyModule);
        }

        public Task<Models.MyModule> UpdateMyModuleAsync(Models.MyModule MyModule)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, MyModule.ModuleId, PermissionNames.Edit))
            {
                MyModule = _MyModuleRepository.UpdateMyModule(MyModule);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "MyModule Updated {MyModule}", MyModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Update Attempt {MyModule}", MyModule);
                MyModule = null;
            }
            return Task.FromResult(MyModule);
        }

        public Task DeleteMyModuleAsync(int MyModuleId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _MyModuleRepository.DeleteMyModule(MyModuleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "MyModule Deleted {MyModuleId}", MyModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Delete Attempt {MyModuleId} {ModuleId}", MyModuleId, ModuleId);
            }
            return Task.CompletedTask;
        }
    }
}
