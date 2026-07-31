using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;
using System.Net;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notifications;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public NotificationController(INotificationRepository notifications, IUserPermissions userPermissions, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _notifications = notifications;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>/read?siteid=x&direction=to&userid=1&count=5&isread=false
        [HttpGet("read")]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Notification> Get(string siteid, string direction, string userid, string count, string isread)
        {
            IEnumerable<Notification> notifications = null;

            if (int.TryParse(siteid, out int SiteId) && SiteId == _alias.SiteId && int.TryParse(userid, out int UserId) && int.TryParse(count, out int Count) && bool.TryParse(isread, out bool IsRead) && IsAuthorized(UserId))
            {
                if (direction == "to")
                {
                    notifications = _notifications.GetNotifications(SiteId, -1, UserId, Count, IsRead);
                }
                else
                {
                    notifications = _notifications.GetNotifications(SiteId, UserId, -1, Count, IsRead);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Get Attempt {SiteId} {Direction} {UserId} {Count} {isRead}", siteid, direction, userid, count, isread);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return notifications;
        }

        // GET: api/<controller>/read?siteid=x&direction=to&userid=1&count=5&isread=false
        [HttpGet("read-count")]
        [Authorize(Roles = RoleNames.Registered)]
        public int Get(string siteid, string direction, string userid, string isread)
        {
            int notificationsCount = 0;

            if (int.TryParse(siteid, out int SiteId) && SiteId == _alias.SiteId && int.TryParse(userid, out int UserId) && bool.TryParse(isread, out bool IsRead) && IsAuthorized(UserId))
            {
                if (direction == "to")
                {
                    notificationsCount = _notifications.GetNotificationCount(SiteId, -1, UserId, IsRead);
                }
                else
                {
                    notificationsCount = _notifications.GetNotificationCount(SiteId, UserId, -1, IsRead);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Get Attempt {SiteId} {Direction} {UserId} {isRead}", siteid, direction, userid, isread);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return notificationsCount;
        }


        // GET: api/<controller>?siteid=x&type=y&userid=z
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Notification> Get(string siteid, string direction, string userid)
        {
            IEnumerable<Notification> notifications = null;

            if (int.TryParse(siteid, out int SiteId) && SiteId == _alias.SiteId && int.TryParse(userid, out int UserId) && IsAuthorized(UserId))
            {
                if (direction == "to")
                {
                    notifications = _notifications.GetNotifications(SiteId, -1, UserId);
                }
                else
                {
                    notifications = _notifications.GetNotifications(SiteId, UserId, -1);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Get Attempt {SiteId} {Direction} {UserId}", siteid, direction, userid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return notifications;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Get(int id)
        {
            var notification = _notifications.GetNotification(id);
            if (notification != null && notification.SiteId == _alias.SiteId && (IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId)))
            {
                return notification;
            }
            else
            {
                if (notification != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Get Attempt {NotificationId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Post([FromBody] Notification notification)
        {
            if (ModelState.IsValid && notification.SiteId == _alias.SiteId && (IsAuthorized(notification.FromUserId) || (notification.FromUserId == null && User.IsInRole(RoleNames.Admin))))
            {
                if (!User.IsInRole(RoleNames.Admin))
                {
                    // content must be HTML encoded for non-admins to prevent HTML injection
                    notification.Subject = WebUtility.HtmlEncode(notification.Subject);
                    notification.Body = WebUtility.HtmlEncode(notification.Body);
                }
                notification = _notifications.AddNotification(notification);
                _syncManager.AddSyncEvent(_alias, EntityNames.Notification, notification.NotificationId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Notification Added {NotificationId}", notification.NotificationId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Post Attempt {Notification}", notification);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                notification = null;
            }
            return notification;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Put(int id, [FromBody] Notification notification)
        {
            var existing = _notifications.GetNotification(id, false);
            if (ModelState.IsValid && notification.SiteId == _alias.SiteId && notification.NotificationId == id && existing != null && existing.SiteId == _alias.SiteId)
            {
                bool update = false;
                if (IsAuthorized(existing.FromUserId))
                {
                    // notification belongs to current authenticated user - update is allowed
                    if (!User.IsInRole(RoleNames.Admin))
                    {
                        // content must be HTML encoded for non-admins to prevent HTML injection
                        notification.Subject = WebUtility.HtmlEncode(notification.Subject);
                        notification.Body = WebUtility.HtmlEncode(notification.Body);
                    }
                    update = true;
                }
                else
                {
                    if (IsAuthorized(existing.ToUserId))
                    {
                        // notification was sent to current authenticated user - only isread and isdeleted properties can be updated
                        existing.IsRead = notification.IsRead;
                        existing.IsDeleted = notification.IsDeleted;
                        notification = existing;
                        update = true;
                    }
                }
                if (update)
                {
                    notification = _notifications.UpdateNotification(notification);
                    _syncManager.AddSyncEvent(_alias, EntityNames.Notification, notification.NotificationId, SyncEventActions.Update);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Notification Updated {NotificationId}", notification.NotificationId);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Put Attempt {Notification}", notification);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    notification = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Put Attempt {Notification}", notification);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                notification = null;
            }
            return notification;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            var notification = _notifications.GetNotification(id);
            if (notification != null && notification.SiteId == _alias.SiteId && (IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId)))
            {
                _notifications.DeleteNotification(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.Notification, notification.NotificationId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Notification Deleted {NotificationId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Delete Attempt {NotificationId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private bool IsAuthorized(int? userid)
        {
            bool authorized = false;
            if (userid != null)
            {
                authorized = (_userPermissions.GetUser(User).UserId == userid);
            }
            return authorized;
        }
    }
}
