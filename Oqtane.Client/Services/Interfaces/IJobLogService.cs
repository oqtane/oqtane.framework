using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to read the job schedule log
    /// </summary>
    public interface IJobLogService
    {
        /// <summary>
        /// Return a list of all <see cref="JobLog"/> entries
        /// </summary>
        /// <returns></returns>
        Task<List<JobLog>> GetJobLogsAsync();

        /// <summary>
        /// Return a <see cref="JobLog"/> entry for the given Id
        /// </summary>
        /// <param name="jobLogId"></param>
        /// <returns></returns>
        Task<JobLog> GetJobLogAsync(int jobLogId);
    }
}
