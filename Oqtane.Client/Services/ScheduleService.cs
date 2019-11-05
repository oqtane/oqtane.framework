using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ScheduleService : ServiceBase, IScheduleService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public ScheduleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Schedule"); }
        }

        public async Task<List<Schedule>> GetSchedulesAsync()
        {
            List<Schedule> schedules = await http.GetJsonAsync<List<Schedule>>(apiurl);
            return schedules.OrderBy(item => item.Name).ToList();
        }

        public async Task<Schedule> GetScheduleAsync(int ScheduleId)
        {
            return await http.GetJsonAsync<Schedule>(apiurl + "/" + ScheduleId.ToString());
        }

        public async Task<Schedule> AddScheduleAsync(Schedule schedule)
        {
            return await http.PostJsonAsync<Schedule>(apiurl, schedule);
        }

        public async Task<Schedule> UpdateScheduleAsync(Schedule schedule)
        {
            return await http.PutJsonAsync<Schedule>(apiurl + "/" + schedule.ScheduleId.ToString(), schedule);
        }
        public async Task DeleteScheduleAsync(int ScheduleId)
        {
            await http.DeleteAsync(apiurl + "/" + ScheduleId.ToString());
        }
    }
}
