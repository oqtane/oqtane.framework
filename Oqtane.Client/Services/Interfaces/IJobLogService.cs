using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IJobLogService
    {
        Task<List<JobLog>> GetJobLogsAsync();

        Task<JobLog> GetJobLogAsync(int jobLogId);

        Task<JobLog> AddJobLogAsync(JobLog jobLog);

        Task<JobLog> UpdateJobLogAsync(JobLog jobLog);

        Task DeleteJobLogAsync(int jobLogId);
    }
}
