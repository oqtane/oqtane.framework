using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage jobs (<see cref="Job"/>)
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// Returns a list of all jobs
        /// </summary>
        /// <returns></returns>
        Task<List<Job>> GetJobsAsync();

        /// <summary>
        /// Return a specific job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<Job> GetJobAsync(int jobId);

        /// <summary>
        /// Adds a new job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<Job> AddJobAsync(Job job);

        /// <summary>
        /// Updates an existing job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<Job> UpdateJobAsync(Job job);

        /// <summary>
        /// Delete an existing job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task DeleteJobAsync(int jobId);

        /// <summary>
        /// Starts the given job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task StartJobAsync(int jobId);

        /// <summary>
        /// Stops the given job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task StopJobAsync(int jobId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class JobService : ServiceBase, IJobService
    {
        public JobService(HttpClient http, SiteState siteState) : base(http, siteState) { }

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
