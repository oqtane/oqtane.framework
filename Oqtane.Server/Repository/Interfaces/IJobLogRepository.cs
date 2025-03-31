using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobLogRepository
    {
        IEnumerable<JobLog> GetJobLogs();
        IEnumerable<JobLog> GetJobLogs(int jobId);
        JobLog AddJobLog(JobLog jobLog);
        JobLog UpdateJobLog(JobLog jobLog);
        JobLog GetJobLog(int jobLogId);
        void DeleteJobLog(int jobLogId);
    }
}
