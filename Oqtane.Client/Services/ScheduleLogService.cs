using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ScheduleLogService : ServiceBase, IScheduleLogService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public ScheduleLogService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "ScheduleLog"); }
        }

        public async Task<List<ScheduleLog>> GetScheduleLogsAsync()
        {
            List<ScheduleLog> schedulelogs = await http.GetJsonAsync<List<ScheduleLog>>(apiurl);
            return schedulelogs.OrderBy(item => item.StartDate).ToList();
        }

        public async Task<ScheduleLog> GetScheduleLogAsync(int ScheduleLogId)
        {
            return await http.GetJsonAsync<ScheduleLog>(apiurl + "/" + ScheduleLogId.ToString());
        }

        public async Task<ScheduleLog> AddScheduleLogAsync(ScheduleLog schedulelog)
        {
            return await http.PostJsonAsync<ScheduleLog>(apiurl, schedulelog);
        }

        public async Task<ScheduleLog> UpdateScheduleLogAsync(ScheduleLog schedulelog)
        {
            return await http.PutJsonAsync<ScheduleLog>(apiurl + "/" + schedulelog.ScheduleLogId.ToString(), schedulelog);
        }
        public async Task DeleteScheduleLogAsync(int ScheduleLogId)
        {
            await http.DeleteAsync(apiurl + "/" + ScheduleLogId.ToString());
        }
    }
}
