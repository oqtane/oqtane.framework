using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface INotificationRepository
    {
        IEnumerable<Notification> GetNotifications(int SiteId, int FromUserId, int ToUserId);
        Notification AddNotification(Notification Notification);
        Notification UpdateNotification(Notification Notification);
        Notification GetNotification(int NotificationId);
        void DeleteNotification(int NotificationId);
    }
}
