using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retrieve notifications (<see cref="Notification"/>)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Return a list of notifications
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="direction"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="direction"></param>
        /// <param name="userId"></param>
        /// <param name="count"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId, int count, bool isRead);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="direction"></param>
        /// <param name="userId"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        Task<int> GetNotificationCountAsync(int siteId, string direction, int userId, bool isRead);

        /// <summary>
        /// Returns a specific notifications
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        Task<Notification> GetNotificationAsync(int notificationId);

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task<Notification> AddNotificationAsync(Notification notification);

        /// <summary>
        /// Updates a existing notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task<Notification> UpdateNotificationAsync(Notification notification);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        Task DeleteNotificationAsync(int notificationId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class NotificationService : ServiceBase, INotificationService
    {
        public NotificationService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Notification");

        public async Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId)
        {
            var notifications = await GetJsonAsync<List<Notification>>($"{Apiurl}?siteid={siteId}&direction={direction.ToLower()}&userid={userId}");

            return notifications.OrderByDescending(item => item.CreatedOn).ToList();
        }

        public async Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId, int count, bool isRead)
        {
            var notifications = await GetJsonAsync<List<Notification>>($"{Apiurl}/read?siteid={siteId}&direction={direction.ToLower()}&userid={userId}&count={count}&isread={isRead}");

            return notifications.OrderByDescending(item => item.CreatedOn).ToList();
        }

        public async Task<int> GetNotificationCountAsync(int siteId, string direction, int userId, bool isRead)
        {
            var notificationCount = await GetJsonAsync<int>($"{Apiurl}/read-count?siteid={siteId}&direction={direction.ToLower()}&userid={userId}&isread={isRead}");

            return notificationCount;
        }

        public async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await GetJsonAsync<Notification>($"{Apiurl}/{notificationId}");
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            return await PostJsonAsync<Notification>(Apiurl, notification);
        }

        public async Task<Notification> UpdateNotificationAsync(Notification notification)
        {
            return await PutJsonAsync<Notification>($"{Apiurl}/{notification.NotificationId}", notification);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            await DeleteAsync($"{Apiurl}/{notificationId}");
        }
    }
}
