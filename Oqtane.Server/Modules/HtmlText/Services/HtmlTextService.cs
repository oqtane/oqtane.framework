using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class ServerHtmlTextService : IHtmlTextService, ITransientService
    {
        private readonly IHtmlTextRepository _htmlTextRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly IMemoryCache _cache;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerHtmlTextService(IHtmlTextRepository htmlTextRepository, IUserPermissions userPermissions, IMemoryCache cache, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _htmlTextRepository = htmlTextRepository;
            _userPermissions = userPermissions;
            _cache = cache;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.HtmlText>> GetHtmlTextsAsync(int moduleId)
        {
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Registered))
            {
                return Task.FromResult(_htmlTextRepository.GetHtmlTexts(moduleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                return null;
            }
        }

        public Task<Models.HtmlText> GetHtmlTextAsync(int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_cache.GetOrCreate($"HtmlText:{_alias.SiteKey}:{moduleId}", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return _htmlTextRepository.GetHtmlText(moduleId);
                }));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                return null;
            }
        }

        public Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmlText)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, htmlText.ModuleId, PermissionNames.Edit))
            {
                htmlText = _htmlTextRepository.AddHtmlText(htmlText);
                ClearCache(htmlText.ModuleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Html/Text Added {HtmlText}", htmlText);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Add Attempt {HtmlText}", htmlText);
                htmlText = null;
            }
            return Task.FromResult(htmlText);
        }

        public Task DeleteHtmlTextAsync(int htmlTextId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _htmlTextRepository.DeleteHtmlText(htmlTextId);
                ClearCache(moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Html/Text Deleted {HtmlTextId}", htmlTextId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Delete Attempt {HtmlTextId} {ModuleId}", htmlTextId, moduleId);
            }
            return Task.CompletedTask;
        }

        private void ClearCache(int moduleId)
        {
            _cache.Remove($"HtmlText:{_alias.SiteKey}:{moduleId}");
        }
    }
}
