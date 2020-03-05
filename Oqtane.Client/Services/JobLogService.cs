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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public JobLogService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "JobLog"); }
        }

        public async Task<List<JobLog>> GetJobLogsAsync()
        {
            List<JobLog> Joblogs = await _http.GetJsonAsync<List<JobLog>>(apiurl);
            return Joblogs.OrderBy(item => item.StartDate).ToList();
        }

        public async Task<JobLog> GetJobLogAsync(int JobLogId)
        {
            return await _http.GetJsonAsync<JobLog>(apiurl + "/" + JobLogId.ToString());
        }

        public async Task<JobLog> AddJobLogAsync(JobLog Joblog)
        {
            return await _http.PostJsonAsync<JobLog>(apiurl, Joblog);
        }

        public async Task<JobLog> UpdateJobLogAsync(JobLog Joblog)
        {
            return await _http.PutJsonAsync<JobLog>(apiurl + "/" + Joblog.JobLogId.ToString(), Joblog);
        }
        public async Task DeleteJobLogAsync(int JobLogId)
        {
            await _http.DeleteAsync(apiurl + "/" + JobLogId.ToString());
        }
    }
}
