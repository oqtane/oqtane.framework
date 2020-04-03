using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IJobService
    {
        Task<List<Job>> GetJobsAsync();

        Task<Job> GetJobAsync(int jobId);

        Task<Job> AddJobAsync(Job job);

        Task<Job> UpdateJobAsync(Job job);

        Task DeleteJobAsync(int jobId);

        Task StartJobAsync(int jobId);

        Task StopJobAsync(int jobId);
    }
}
