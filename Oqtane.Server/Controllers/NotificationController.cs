using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notifications;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public NotificationController(INotificationRepository notifications, IUserPermissions userPermissions, ILogManager logger)
        {
            _notifications = notifications;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x&type=y&userid=z
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Notification> Get(string siteid, string direction, string userid)
        {
            IEnumerable<Notification> notifications = null;
            if (IsAuthorized(int.Parse(userid)))
            {
                if (direction == "to")
                {
                    notifications = _notifications.GetNotifications(int.Parse(siteid), -1, int.Parse(userid));
                }
                else
                {
                    notifications = _notifications.GetNotifications(int.Parse(siteid), int.Parse(userid), -1);
                }
            }
            return notifications;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Get(int id)
        {
            Notification notification = _notifications.GetNotification(id);
            if (!(IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId)))
            {
                notification = null;
            }
            return notification;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Post([FromBody] Notification notification)
        {
            if (IsAuthorized(notification.FromUserId))
            {
                notification = _notifications.AddNotification(notification);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Notification Added {Notification}", notification);
            }
            return notification;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Notification Put(int id, [FromBody] Notification notification)
        {
            if (IsAuthorized(notification.FromUserId))
            {
                notification = _notifications.UpdateNotification(notification);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Notification Updated {Folder}", notification);
            }
            return notification;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            Notification notification = _notifications.GetNotification(id);
            if (IsAuthorized(notification.FromUserId) || IsAuthorized(notification.ToUserId))
            {
                _notifications.DeleteNotification(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Notification Deleted {NotificationId}", id);
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
