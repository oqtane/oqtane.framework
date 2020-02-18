using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Microsoft.AspNetCore.Http;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository Notifications;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public NotificationController(INotificationRepository Notifications, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.Notifications = Notifications;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        // GET: api/<controller>?siteid=x&type=y&userid=z
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<Notification> Get(string siteid, string direction, string userid)
        {
            IEnumerable<Notification> notifications = null;
            if (IsAuthorized(int.Parse(userid)))
            {
                if (direction == "to")
                {
                    notifications = Notifications.GetNotifications(int.Parse(siteid), -1, int.Parse(userid));
                }
                else
                {
                    notifications = Notifications.GetNotifications(int.Parse(siteid), int.Parse(userid), -1);
                }
            }
            return notifications;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Notification Get(int id)
        {
            Notification Notification = Notifications.GetNotification(id);
            if (!(IsAuthorized(Notification.FromUserId) || IsAuthorized(Notification.ToUserId)))
            {
                Notification = null;
            }
            return Notification;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Notification Post([FromBody] Notification Notification)
        {
            if (IsAuthorized(Notification.FromUserId))
            {
                Notification = Notifications.AddNotification(Notification);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Notification Added {Notification}", Notification);
            }
            return Notification;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Notification Put(int id, [FromBody] Notification Notification)
        {
            if (IsAuthorized(Notification.FromUserId))
            {
                Notification = Notifications.UpdateNotification(Notification);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Notification Updated {Folder}", Notification);
            }
            return Notification;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            Notification Notification = Notifications.GetNotification(id);
            if (IsAuthorized(Notification.FromUserId) || IsAuthorized(Notification.ToUserId))
            {
                Notifications.DeleteNotification(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Notification Deleted {NotificationId}", id);
            }
        }

        private bool IsAuthorized(int? userid)
        {
            bool authorized = true;
            if (userid != null)
            {
                authorized = (UserPermissions.GetUser(User).UserId == userid);
            }
            return authorized;
        }

    }
}
