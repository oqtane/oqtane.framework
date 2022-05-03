using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Documentation;

namespace Oqtane.Services
{
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
