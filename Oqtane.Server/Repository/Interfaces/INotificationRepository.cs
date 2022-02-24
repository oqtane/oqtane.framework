using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface INotificationRepository
    {
        IEnumerable<Notification> GetNotifications(int siteId, int fromUserId, int toUserId);
        Notification AddNotification(Notification notification);
        Notification UpdateNotification(Notification notification);
        Notification GetNotification(int notificationId);
        Notification GetNotification(int notificationId, bool tracking);
        void DeleteNotification(int notificationId);
        int DeleteNotifications(int siteId, int age);
    }
}
