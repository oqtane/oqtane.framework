using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private TenantDBContext _db;

        public NotificationRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId)
        {
            if (toUserId == -1 && fromUserId == -1)
            {
                return _db.Notification
                    .Where(item => item.SiteId == siteId)
                    .Where(item => item.IsDelivered == false && item.IsDeleted == false)
                    .Where(item => item.SendOn == null || item.SendOn < System.DateTime.UtcNow)
                    .ToList();
            }

            return _db.Notification
                .Where(item => item.SiteId == siteId)
                .Where(item => item.ToUserId == toUserId || toUserId == -1)
                .Where(item => item.FromUserId == fromUserId || fromUserId == -1)
                .ToList();
        }

        public Notification AddNotification(Notification notification)
        {
            _db.Notification.Add(notification);
            _db.SaveChanges();
            return notification;
        }

        public Notification UpdateNotification(Notification notification)
        {
            _db.Entry(notification).State = EntityState.Modified;
            _db.SaveChanges();
            return notification;
        }
        public Notification GetNotification(int notificationId)
        {
            return GetNotification(notificationId, true);
        }

        public Notification GetNotification(int notificationId, bool tracking)
        {
            if (tracking)
            {
                return _db.Notification.Find(notificationId);
            }
            else
            {
                return _db.Notification.AsNoTracking().FirstOrDefault(item => item.NotificationId == notificationId);
            }
        }

        public void DeleteNotification(int notificationId)
        {
            Notification notification = _db.Notification.Find(notificationId);
            _db.Notification.Remove(notification);
            _db.SaveChanges();
        }

        public int DeleteNotifications(int siteId, int age)
        {
            // delete notifications in batches of 100 records
            int count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var notifications = _db.Notification.Where(item => item.SiteId == siteId && item.FromUserId == null && item.IsDelivered && item.DeliveredOn < purgedate)
                .OrderBy(item => item.DeliveredOn).Take(100).ToList();
            while (notifications.Count > 0)
            {
                count += notifications.Count;
                _db.Notification.RemoveRange(notifications);
                _db.SaveChanges();
                notifications = _db.Notification.Where(item => item.SiteId == siteId && item.FromUserId == null && item.IsDelivered && item.DeliveredOn < purgedate)
                .OrderBy(item => item.DeliveredOn).Take(100).ToList();
            }
            return count;
        }

    }

}
