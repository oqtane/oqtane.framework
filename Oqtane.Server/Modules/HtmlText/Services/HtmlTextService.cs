using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class ServerHtmlTextService : IHtmlTextService, IServerService
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly IUserPermissions _userPermissions;
        private readonly ITenantManager _tenantManager;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;

        public ServerHtmlTextService(IHtmlTextRepository htmlText, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _htmlText = htmlText;
            _userPermissions = userPermissions;
            _tenantManager = tenantManager;
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<List<Models.HtmlText>> GetHtmlTextsAsync(int moduleId)
        {
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Registered))
            {
                return (await _htmlText.GetHtmlTextsAsync(moduleId)).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                return null;
            }
        }

        public async Task<Models.HtmlText> GetHtmlTextAsync(int moduleId)
        {
            var alias = _tenantManager.GetAlias();
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                var htmltexts = await _htmlText.GetHtmlTextsAsync(moduleId);
                if (htmltexts != null && htmltexts.Any())
                {
                    return htmltexts.OrderByDescending(item => item.CreatedOn).First();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                return null;
            }
        }

        public async Task<Models.HtmlText> GetHtmlTextAsync(int htmlTextId, int moduleId)
        {
            var alias = _tenantManager.GetAlias();
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return await _htmlText.GetHtmlTextAsync(htmlTextId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {HtmlTextId} {ModuleId}", htmlTextId, moduleId);
                return null;
            }
        }

        public async Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmlText)
        {
            var alias = _tenantManager.GetAlias();
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, alias.SiteId, EntityNames.Module, htmlText.ModuleId, PermissionNames.Edit))
            {
                return await _htmlText.AddHtmlTextAsync(htmlText);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Add Attempt {HtmlText}", htmlText);
                return null;
            }
        }

        public async Task DeleteHtmlTextAsync(int htmlTextId, int moduleId)
        {
            var alias = _tenantManager.GetAlias();
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                await _htmlText.DeleteHtmlTextAsync(htmlTextId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Delete Attempt {HtmlTextId} {ModuleId}", htmlTextId, moduleId);
            }
        }
    }
}
