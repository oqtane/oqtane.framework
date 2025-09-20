using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface INotificationRepository
    {
        IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId);
        IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId, int count, bool isRead);
        int GetNotificationCount(int siteId, int fromUserId, int toUserId, bool isRead);
        Notification AddNotification(Notification notification);
        Notification UpdateNotification(Notification notification);
        Notification GetNotification(int notificationId);
        Notification GetNotification(int notificationId, bool tracking);
        void DeleteNotification(int notificationId);
        int DeleteNotifications(int siteId, int age);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public NotificationRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (toUserId == -1 && fromUserId == -1)
            {
                return db.Notification
                    .Where(item => item.SiteId == siteId)
                    .Where(item => item.IsDelivered == false && item.IsDeleted == false)
                    .Where(item => item.SendOn == null || item.SendOn < System.DateTime.UtcNow)
                    .ToList();
            }

            return db.Notification
                .Where(item => item.SiteId == siteId)
                .Where(item => item.ToUserId == toUserId || toUserId == -1)
                .Where(item => item.FromUserId == fromUserId || fromUserId == -1)
                .ToList();
        }

        public IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId, int count, bool isRead)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (toUserId == -1 && fromUserId == -1)
            {
                return db.Notification
                    .Where(item => item.SiteId == siteId)
                    .Where(item => item.IsDelivered == false && item.IsDeleted == false)
                    .Where(item => item.SendOn == null || item.SendOn < System.DateTime.UtcNow)
                    .Where(item => item.IsRead == isRead)
                    .OrderByDescending(item => item.CreatedOn)
                    .ToList()
                    .Take(count);
            }

            return db.Notification
                .Where(item => item.SiteId == siteId)
                .Where(item => item.ToUserId == toUserId || toUserId == -1)
                .Where(item => item.FromUserId == fromUserId || fromUserId == -1)
                .Where(item => item.IsRead == isRead)
                .OrderByDescending(item => item.CreatedOn)
                .ToList()
                .Take(count);
        }

        public int GetNotificationCount(int siteId, int fromUserId, int toUserId, bool isRead)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (toUserId == -1 && fromUserId == -1)
            {
                return db.Notification
                    .Where(item => item.SiteId == siteId)
                    .Where(item => item.IsDelivered == false && item.IsDeleted == false)
                    .Where(item => item.SendOn == null || item.SendOn < System.DateTime.UtcNow)
                    .Where(item => item.IsRead == isRead)
                    .ToList()
                    .Count();

            }

            return db.Notification
                .Where(item => item.SiteId == siteId)
                .Where(item => item.ToUserId == toUserId || toUserId == -1)
                .Where(item => item.FromUserId == fromUserId || fromUserId == -1)
                .Where(item => item.IsRead == isRead)
                .ToList()
                .Count();
        }


        public Notification AddNotification(Notification notification)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Notification.Add(notification);
            db.SaveChanges();
            return notification;
        }

        public Notification UpdateNotification(Notification notification)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(notification).State = EntityState.Modified;
            db.SaveChanges();
            return notification;
        }
        public Notification GetNotification(int notificationId)
        {
            return GetNotification(notificationId, true);
        }

        public Notification GetNotification(int notificationId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.Notification.Find(notificationId);
            }
            else
            {
                return db.Notification.AsNoTracking().FirstOrDefault(item => item.NotificationId == notificationId);
            }
        }

        public void DeleteNotification(int notificationId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var notification = db.Notification.Find(notificationId);
            db.Notification.Remove(notification);
            db.SaveChanges();
        }

        public int DeleteNotifications(int siteId, int age)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete notifications in batches of 100 records
            var count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var notifications = db.Notification.Where(item => item.SiteId == siteId && item.FromUserId == null && (item.IsDeleted || item.IsDelivered && item.DeliveredOn < purgedate))
                .OrderBy(item => item.DeliveredOn).Take(100).ToList();
            while (notifications.Count > 0)
            {
                count += notifications.Count;
                db.Notification.RemoveRange(notifications);
                db.SaveChanges();
                notifications = db.Notification.Where(item => item.SiteId == siteId && item.FromUserId == null && (item.IsDeleted || item.IsDelivered && item.DeliveredOn < purgedate))
                .OrderBy(item => item.DeliveredOn).Take(100).ToList();
            }
            return count;
        }

    }

}
