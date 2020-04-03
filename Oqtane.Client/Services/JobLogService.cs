using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Services.Interfaces;
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

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "JobLog"); }
        }

        public async Task<List<JobLog>> GetJobLogsAsync()
        {
            List<JobLog> joblogs = await _http.GetJsonAsync<List<JobLog>>(Apiurl);
            return joblogs.OrderBy(item => item.StartDate).ToList();
        }

        public async Task<JobLog> GetJobLogAsync(int jobLogId)
        {
            return await _http.GetJsonAsync<JobLog>($"{Apiurl}/{jobLogId.ToString()}");
        }

        public async Task<JobLog> AddJobLogAsync(JobLog joblog)
        {
            return await _http.PostJsonAsync<JobLog>(Apiurl, joblog);
        }

        public async Task<JobLog> UpdateJobLogAsync(JobLog joblog)
        {
            return await _http.PutJsonAsync<JobLog>($"{Apiurl}/{joblog.JobLogId.ToString()}", joblog);
        }
        public async Task DeleteJobLogAsync(int jobLogId)
        {
            await _http.DeleteAsync($"{Apiurl}/{jobLogId.ToString()}");
        }
    }
}
