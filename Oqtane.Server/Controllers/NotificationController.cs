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
using System.Reflection.Metadata;

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

        // GET: api/<controller>?siteid=x&type=y&userid=z
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Notification> Get(string siteid, string direction, string userid)
        {
            IEnumerable<Notification> notifications = null;

            int SiteId;
            int UserId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId && int.TryParse(userid, out UserId) && IsAuthorized(UserId))
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
                notifications = null;
            }

            return notifications;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Get(int id)
        {
            Notification notification = _notifications.GetNotification(id);
            if (notification != null && notification.SiteId == _alias.SiteId && (IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId)))
            {
                return notification;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Notification Get Attempt {NotificationId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Post([FromBody] Notification notification)
        {
            if (ModelState.IsValid && notification.SiteId == _alias.SiteId && IsAuthorized(notification.FromUserId))
            {
                notification = _notifications.AddNotification(notification);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Notification, notification.NotificationId, SyncEventActions.Create);
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
            if (ModelState.IsValid && notification.SiteId == _alias.SiteId && _notifications.GetNotification(notification.NotificationId, false) != null && IsAuthorized(notification.FromUserId))
            {
                notification = _notifications.UpdateNotification(notification);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Notification, notification.NotificationId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Notification Updated {NotificationId}", notification.NotificationId);
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
            Notification notification = _notifications.GetNotification(id);
            if (notification != null && notification.SiteId == _alias.SiteId && (IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId)))
            {
                _notifications.DeleteNotification(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Notification, notification.NotificationId, SyncEventActions.Delete);
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
            bool authorized = true;
            if (userid != null)
            {
                authorized = (_userPermissions.GetUser(User).UserId == userid);
            }
            return authorized;
        }

    }
}
