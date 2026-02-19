using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage tasks (<see cref="JobTask"/>)
    /// </summary>
    public interface IJobTaskService
    {
        /// <summary>
        /// Return a specific task
        /// </summary>
        /// <param name="jobTaskId"></param>
        /// <returns></returns>
        Task<JobTask> GetJobTaskAsync(int jobTaskId);

        /// <summary>
        /// Adds a new task
        /// </summary>
        /// <param name="jobTask"></param>
        /// <returns></returns>
        Task<JobTask> AddJobTaskAsync(JobTask jobTask);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class JobTaskService : ServiceBase, IJobTaskService
    {
        public JobTaskService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("JobTask");

        public async Task<JobTask> GetJobTaskAsync(int jobTaskId)
        {
            return await GetJsonAsync<JobTask>($"{Apiurl}/{jobTaskId}");
        }

        public async Task<JobTask> AddJobTaskAsync(JobTask jobTask)
        {
            return await PostJsonAsync<JobTask>(Apiurl, jobTask);
        }
    }
}
