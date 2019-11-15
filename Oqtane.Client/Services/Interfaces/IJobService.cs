using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IJobService
    {
        Task<List<Job>> GetJobsAsync();

        Task<Job> GetJobAsync(int JobId);

        Task<Job> AddJobAsync(Job Job);

        Task<Job> UpdateJobAsync(Job Job);

        Task DeleteJobAsync(int JobId);

        Task StartJobAsync(int JobId);

        Task StopJobAsync(int JobId);
    }
}
