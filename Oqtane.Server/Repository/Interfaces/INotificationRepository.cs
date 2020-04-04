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
        void DeleteNotification(int notificationId);
    }
}
