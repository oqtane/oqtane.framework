using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private TenantDBContext db;

        public NotificationRepository(TenantDBContext context)
        {
            db = context;
        }
            
        public IEnumerable<Notification> GetNotifications(int SiteId, int FromUserId, int ToUserId)
        {
            if (ToUserId == -1 && FromUserId == -1)
            {
                return db.Notification
                    .Where(item => item.SiteId == SiteId)
                    .Where(item => item.IsDelivered == false)
                    .Include(item => item.FromUser)
                    .Include(item => item.ToUser)
                    .ToList();
            }
            else
            {
                return db.Notification
                    .Where(item => item.SiteId == SiteId)
                    .Where(item => item.ToUserId == ToUserId || ToUserId == -1)
                    .Where(item => item.FromUserId == FromUserId || FromUserId == -1)
                    .Include(item => item.FromUser)
                    .Include(item => item.ToUser)
                    .ToList();
            }
        }

        public Notification AddNotification(Notification Notification)
        {
            db.Notification.Add(Notification);
            db.SaveChanges();
            return Notification;
        }

        public Notification UpdateNotification(Notification Notification)
        {
            db.Entry(Notification).State = EntityState.Modified;
            db.SaveChanges();
            return Notification;
        }

        public Notification GetNotification(int NotificationId)
        {
            return db.Notification.Find(NotificationId);
        }

        public void DeleteNotification(int NotificationId)
        {
            Notification Notification = db.Notification.Find(NotificationId);
            db.Notification.Remove(Notification);
            db.SaveChanges();
        }
    }

}
