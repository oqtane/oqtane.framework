using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId);

        Task<Notification> GetNotificationAsync(int notificationId);

        Task<Notification> AddNotificationAsync(Notification notification);

        Task<Notification> UpdateNotificationAsync(Notification notification);

        Task DeleteNotificationAsync(int notificationId);
    }
}
