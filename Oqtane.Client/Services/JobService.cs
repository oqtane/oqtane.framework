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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public JobService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Job"); }
        }

        public async Task<List<Job>> GetJobsAsync()
        {
            List<Job> Jobs = await http.GetJsonAsync<List<Job>>(apiurl);
            return Jobs.OrderBy(item => item.Name).ToList();
        }

        public async Task<Job> GetJobAsync(int JobId)
        {
            return await http.GetJsonAsync<Job>(apiurl + "/" + JobId.ToString());
        }

        public async Task<Job> AddJobAsync(Job Job)
        {
            return await http.PostJsonAsync<Job>(apiurl, Job);
        }

        public async Task<Job> UpdateJobAsync(Job Job)
        {
            return await http.PutJsonAsync<Job>(apiurl + "/" + Job.JobId.ToString(), Job);
        }
        public async Task DeleteJobAsync(int JobId)
        {
            await http.DeleteAsync(apiurl + "/" + JobId.ToString());
        }

        public async Task StartJobAsync(int JobId)
        {
            await http.GetAsync(apiurl + "/start/" + JobId.ToString());
        }

        public async Task StopJobAsync(int JobId)
        {
            await http.GetAsync(apiurl + "/stop/" + JobId.ToString());
        }
    }
}
