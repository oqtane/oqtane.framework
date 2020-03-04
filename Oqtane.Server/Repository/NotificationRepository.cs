using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            
        public IEnumerable<Notification> GetNotifications(int SiteId, int FromUserId, int ToUserId)
        {
            if (ToUserId == -1 && FromUserId == -1)
            {
                return _db.Notification
                    .Where(item => item.SiteId == SiteId)
                    .Where(item => item.IsDelivered == false)
                    .Include(item => item.FromUser)
                    .Include(item => item.ToUser)
                    .ToList();
            }
            else
            {
                return _db.Notification
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
            _db.Notification.Add(Notification);
            _db.SaveChanges();
            return Notification;
        }

        public Notification UpdateNotification(Notification Notification)
        {
            _db.Entry(Notification).State = EntityState.Modified;
            _db.SaveChanges();
            return Notification;
        }

        public Notification GetNotification(int NotificationId)
        {
            return _db.Notification.Find(NotificationId);
        }

        public void DeleteNotification(int NotificationId)
        {
            Notification Notification = _db.Notification.Find(NotificationId);
            _db.Notification.Remove(Notification);
            _db.SaveChanges();
        }
    }

}
