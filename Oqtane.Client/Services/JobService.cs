using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class JobService : ServiceBase, IJobService
    {
        public JobService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("Job");

        public async Task<List<Job>> GetJobsAsync()
        {
            List<Job> jobs = await GetJsonAsync<List<Job>>(Apiurl);
            return jobs.OrderBy(item => item.Name).ToList();
        }

        public async Task<Job> GetJobAsync(int jobId)
        {
            return await GetJsonAsync<Job>($"{Apiurl}/{jobId}");
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            return await PostJsonAsync<Job>(Apiurl, job);
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            return await PutJsonAsync<Job>($"{Apiurl}/{job.JobId}", job);
        }
        public async Task DeleteJobAsync(int jobId)
        {
            await DeleteAsync($"{Apiurl}/{jobId}");
        }

        public async Task StartJobAsync(int jobId)
        {
            await GetAsync($"{Apiurl}/start/{jobId}");
        }

        public async Task StopJobAsync(int jobId)
        {
            await GetAsync($"{Apiurl}/stop/{jobId}");
        }
    }
}
