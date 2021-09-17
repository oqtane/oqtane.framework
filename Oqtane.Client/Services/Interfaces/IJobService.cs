using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
}
