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
    public class JobService : ServiceBase, IJobService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public JobService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Job"); }
        }

        public async Task<List<Job>> GetJobsAsync()
        {
            List<Job> jobs = await _http.GetJsonAsync<List<Job>>(Apiurl);
            return jobs.OrderBy(item => item.Name).ToList();
        }

        public async Task<Job> GetJobAsync(int jobId)
        {
            return await _http.GetJsonAsync<Job>($"{Apiurl}/{jobId.ToString()}");
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            return await _http.PostJsonAsync<Job>(Apiurl, job);
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            return await _http.PutJsonAsync<Job>($"{Apiurl}/{job.JobId.ToString()}", job);
        }
        public async Task DeleteJobAsync(int jobId)
        {
            await _http.DeleteAsync($"{Apiurl}/{jobId.ToString()}");
        }

        public async Task StartJobAsync(int jobId)
        {
            await _http.GetAsync($"{Apiurl}/start/{jobId.ToString()}");
        }

        public async Task StopJobAsync(int jobId)
        {
            await _http.GetAsync($"{Apiurl}/stop/{jobId.ToString()}");
        }
    }
}
