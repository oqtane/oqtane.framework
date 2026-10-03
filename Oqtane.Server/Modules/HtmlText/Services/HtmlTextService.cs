using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class ServerHtmlTextService : IHtmlTextService, ITransientService
    {
        private readonly IHtmlTextRepository _htmlTextRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ISettingRepository _settingRepository;
        private readonly IPageModuleRepository _pageModuleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ICacheManager _cache;
        private readonly IStringLocalizer<HtmlTextService> _localizer;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerHtmlTextService(IHtmlTextRepository htmlTextRepository, IUserPermissions userPermissions, ISettingRepository settingRepository, IPageModuleRepository pageModuleRepository, IUserRoleRepository userRoleRepository, IUserRepository userRepository, INotificationRepository notificationRepository, ISiteRepository siteRepository, ICacheManager cache, ITenantManager tenantManager, IStringLocalizer<HtmlTextService> localizer, ILogManager logger, IHttpContextAccessor accessor)
        {
            _htmlTextRepository = htmlTextRepository;
            _userPermissions = userPermissions;
            _settingRepository = settingRepository;
            _pageModuleRepository = pageModuleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _siteRepository = siteRepository;
            _cache = cache;
            _localizer = localizer;
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
            return GetHtmlTextAsync(moduleId, WorkflowState.Published);
        }

        public Task<Models.HtmlText> GetHtmlTextAsync(int moduleId, int status)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                if (status == WorkflowState.Published)
                {
                    return Task.FromResult(_cache.GetCache(_alias, $"HtmlText:{moduleId}", entry =>
                    {
                        return _htmlTextRepository.GetHtmlText(moduleId, status);
                    }));
                }
                else
                {
                    return Task.FromResult(_htmlTextRepository.GetHtmlText(moduleId, status));
                }
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

        public Task<Models.HtmlText> UpdateHtmlTextAsync(Models.HtmlText htmlText)
        {
            var workflow = _accessor.HttpContext.GetSiteSettings().GetValue("HtmlText:Workflow", WorkflowType.DirectPublish);
            var approver = _accessor.HttpContext.GetSiteSettings().GetValue("HtmlText:Approver", "");

            var authorized = _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, htmlText.ModuleId, PermissionNames.Edit);
            if (!authorized)
            {
                if (workflow != WorkflowType.DirectPublish && !string.IsNullOrEmpty(approver))
                {
                    authorized = _accessor.HttpContext.User.IsInRole(approver);
                }
            }

            if (authorized)
            {
                htmlText = _htmlTextRepository.UpdateHtmlText(htmlText);

                if (workflow != WorkflowType.DirectPublish)
                {
                    var sitename = _siteRepository.GetSite(_alias.SiteId).Name;
                    var url = "";
                    var pageModule = _pageModuleRepository.GetPageModules(_alias.SiteId).Where(item => item.ModuleId == htmlText.ModuleId).FirstOrDefault();
                    if (pageModule != null)
                    {
                        url = $"https://{_alias.Name}/{pageModule.Page.Path}";
                    }

                    if (htmlText.State == WorkflowState.Review)
                    {
                        if (string.IsNullOrEmpty(htmlText.Comment))
                        {
                            // send to approvers
                            foreach (var userRole in _userRoleRepository.GetUserRoles(_alias.SiteId).Where(item => item.Role.Name == approver))
                            {
                                string subject = _localizer["ContentApprovalEmailSubject"];
                                string body = _localizer["ContentApprovalEmailBody"];
                                body = body.Replace("[UserDisplayName]", userRole.User.DisplayName);
                                body = body.Replace("[Url]", url);
                                body = body.Replace("[SiteName]", sitename);
                                var notification = new Notification(_alias.SiteId, userRole.User, subject, body);
                                _notificationRepository.AddNotification(notification);
                            }
                        }
                        else
                        {
                            // send to creator
                            var user = _userRepository.GetUser(htmlText.CreatedBy);
                            if (user != null)
                            {
                                string subject = _localizer["ContentRejectedEmailSubject"];
                                string body = _localizer["ContentRejectedEmailBody"];
                                body = body.Replace("[UserDisplayName]", user.DisplayName);
                                body = body.Replace("[Comment]", htmlText.Comment);
                                body = body.Replace("[Url]", url);
                                body = body.Replace("[SiteName]", sitename);
                                var notification = new Notification(_alias.SiteId, user, subject, body);
                                _notificationRepository.AddNotification(notification);
                            }
                        }
                    }
                    else // content has been published
                    {                        
                        // send to creator
                        var user = _userRepository.GetUser(htmlText.CreatedBy);
                        if (user != null)
                        {
                            string subject = _localizer["ContentPublishedEmailSubject"];
                            string body = _localizer["ContentPublishedEmailBody"];
                            body = body.Replace("[UserDisplayName]", user.DisplayName);
                            body = body.Replace("[Url]", url);
                            body = body.Replace("[SiteName]", sitename);
                            var notification = new Notification(_alias.SiteId, user, subject, body);
                            _notificationRepository.AddNotification(notification);
                        }
                    }
                }

                ClearCache(htmlText.ModuleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Html/Text Updated {HtmlText}", htmlText);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Update Attempt {HtmlText}", htmlText);
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
            _cache.RemoveCache(_alias, $"HtmlText:{moduleId}");
        }
    }
}
