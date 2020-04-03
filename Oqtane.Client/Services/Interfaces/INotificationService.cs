using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
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
