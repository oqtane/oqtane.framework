using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class JobService : ServiceBase, IJobService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public JobService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            _http = http;
            _siteState = sitestate;
            _navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Job"); }
        }

        public async Task<List<Job>> GetJobsAsync()
        {
            List<Job> Jobs = await _http.GetJsonAsync<List<Job>>(apiurl);
            return Jobs.OrderBy(item => item.Name).ToList();
        }

        public async Task<Job> GetJobAsync(int JobId)
        {
            return await _http.GetJsonAsync<Job>(apiurl + "/" + JobId.ToString());
        }

        public async Task<Job> AddJobAsync(Job Job)
        {
            return await _http.PostJsonAsync<Job>(apiurl, Job);
        }

        public async Task<Job> UpdateJobAsync(Job Job)
        {
            return await _http.PutJsonAsync<Job>(apiurl + "/" + Job.JobId.ToString(), Job);
        }
        public async Task DeleteJobAsync(int JobId)
        {
            await _http.DeleteAsync(apiurl + "/" + JobId.ToString());
        }

        public async Task StartJobAsync(int JobId)
        {
            await _http.GetAsync(apiurl + "/start/" + JobId.ToString());
        }

        public async Task StopJobAsync(int JobId)
        {
            await _http.GetAsync(apiurl + "/stop/" + JobId.ToString());
        }
    }
}
