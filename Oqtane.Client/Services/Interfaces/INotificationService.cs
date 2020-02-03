using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync(int SiteId, string Direction, int UserId);

        Task<Notification> GetNotificationAsync(int NotificationId);

        Task<Notification> AddNotificationAsync(Notification Notification);

        Task<Notification> UpdateNotificationAsync(Notification Notification);

        Task DeleteNotificationAsync(int NotificationId);
    }
}
