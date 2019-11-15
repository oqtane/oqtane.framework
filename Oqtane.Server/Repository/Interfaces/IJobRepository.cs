using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobRepository
    {
        IEnumerable<Job> GetJobs();
        Job AddJob(Job Job);
        Job UpdateJob(Job Job);
        Job GetJob(int JobId);
        void DeleteJob(int JobId);
    }
}
