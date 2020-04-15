using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Services
{
    public class NotificationService : ServiceBase, INotificationService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public NotificationService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Notification"); }
        }

        public async Task<List<Notification>> GetNotificationsAsync(int siteId, string direction, int userId)
        {
            var notifications = await _http.GetJsonAsync<List<Notification>>($"{Apiurl}?siteid={siteId.ToString()}&direction={direction.ToLower()}&userid={userId.ToString()}");
            
            return notifications.OrderByDescending(item => item.CreatedOn).ToList();
        }

        public async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await _http.GetJsonAsync<Notification>($"{Apiurl}/{notificationId.ToString()}");
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            return await _http.PostJsonAsync<Notification>(Apiurl, notification);
        }

        public async Task<Notification> UpdateNotificationAsync(Notification notification)
        {
            return await _http.PutJsonAsync<Notification>($"{Apiurl}/{notification.NotificationId.ToString()}", notification);
        }
        public async Task DeleteNotificationAsync(int notificationId)
        {
            await _http.DeleteAsync($"{Apiurl}/{notificationId.ToString()}");
        }
    }
}
