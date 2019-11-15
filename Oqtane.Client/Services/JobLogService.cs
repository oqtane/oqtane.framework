using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class JobLogService : ServiceBase, IJobLogService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public JobLogService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "JobLog"); }
        }

        public async Task<List<JobLog>> GetJobLogsAsync()
        {
            List<JobLog> Joblogs = await http.GetJsonAsync<List<JobLog>>(apiurl);
            return Joblogs.OrderBy(item => item.StartDate).ToList();
        }

        public async Task<JobLog> GetJobLogAsync(int JobLogId)
        {
            return await http.GetJsonAsync<JobLog>(apiurl + "/" + JobLogId.ToString());
        }

        public async Task<JobLog> AddJobLogAsync(JobLog Joblog)
        {
            return await http.PostJsonAsync<JobLog>(apiurl, Joblog);
        }

        public async Task<JobLog> UpdateJobLogAsync(JobLog Joblog)
        {
            return await http.PutJsonAsync<JobLog>(apiurl + "/" + Joblog.JobLogId.ToString(), Joblog);
        }
        public async Task DeleteJobLogAsync(int JobLogId)
        {
            await http.DeleteAsync(apiurl + "/" + JobLogId.ToString());
        }
    }
}
