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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public NotificationService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Notification"); }
        }

        public async Task<List<Notification>> GetNotificationsAsync(int SiteId, string Direction, int UserId)
        {
            string querystring = "?siteid=" + SiteId.ToString() + "&direction=" + Direction.ToLower() + "&userid=" + UserId.ToString();
            List<Notification> Notifications = await http.GetJsonAsync<List<Notification>>(apiurl + querystring);
            return Notifications.OrderByDescending(item => item.CreatedOn).ToList();
        }

        public async Task<Notification> GetNotificationAsync(int NotificationId)
        {
            return await http.GetJsonAsync<Notification>(apiurl + "/" + NotificationId.ToString());
        }

        public async Task<Notification> AddNotificationAsync(Notification Notification)
        {
            return await http.PostJsonAsync<Notification>(apiurl, Notification);
        }

        public async Task<Notification> UpdateNotificationAsync(Notification Notification)
        {
            return await http.PutJsonAsync<Notification>(apiurl + "/" + Notification.NotificationId.ToString(), Notification);
        }
        public async Task DeleteNotificationAsync(int NotificationId)
        {
            await http.DeleteAsync(apiurl + "/" + NotificationId.ToString());
        }
    }
}
