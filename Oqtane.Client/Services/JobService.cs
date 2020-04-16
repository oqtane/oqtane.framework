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
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public JobService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Job"); }
        }

        public async Task<List<Job>> GetJobsAsync()
        {
            List<Job> jobs = await GetJsonAsync<List<Job>>(Apiurl);
            return jobs.OrderBy(item => item.Name).ToList();
        }

        public async Task<Job> GetJobAsync(int jobId)
        {
            return await GetJsonAsync<Job>($"{Apiurl}/{jobId.ToString()}");
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            return await PostJsonAsync<Job>(Apiurl, job);
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            return await PutJsonAsync<Job>($"{Apiurl}/{job.JobId.ToString()}", job);
        }
        public async Task DeleteJobAsync(int jobId)
        {
            await DeleteAsync($"{Apiurl}/{jobId.ToString()}");
        }

        public async Task StartJobAsync(int jobId)
        {
            await GetAsync($"{Apiurl}/start/{jobId.ToString()}");
        }

        public async Task StopJobAsync(int jobId)
        {
            await GetAsync($"{Apiurl}/stop/{jobId.ToString()}");
        }
    }
}
