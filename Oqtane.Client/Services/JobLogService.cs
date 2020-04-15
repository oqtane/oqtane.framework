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
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public JobLogService(HttpClient http, SiteState siteState, NavigationManager navigationManager) :base(http)    

        {
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "JobLog"); }
        }

        public async Task<List<JobLog>> GetJobLogsAsync()
        {
            List<JobLog> joblogs = await GetJsonAsync<List<JobLog>>(Apiurl);
            return joblogs.OrderBy(item => item.StartDate).ToList();
        }

        public async Task<JobLog> GetJobLogAsync(int jobLogId)
        {
            return await GetJsonAsync<JobLog>($"{Apiurl}/{jobLogId.ToString()}");
        }

        public async Task<JobLog> AddJobLogAsync(JobLog joblog)
        {
            return await PostJsonAsync<JobLog>(Apiurl, joblog);
        }

        public async Task<JobLog> UpdateJobLogAsync(JobLog joblog)
        {
            return await PutJsonAsync<JobLog>($"{Apiurl}/{joblog.JobLogId.ToString()}", joblog);
        }
        public async Task DeleteJobLogAsync(int jobLogId)
        {
            await DeleteAsync($"{Apiurl}/{jobLogId.ToString()}");
        }
    }
}
