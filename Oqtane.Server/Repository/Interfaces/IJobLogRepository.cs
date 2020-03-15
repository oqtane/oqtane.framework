using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobLogRepository
    {
        IEnumerable<JobLog> GetJobLogs();
        JobLog AddJobLog(JobLog jobLog);
        JobLog UpdateJobLog(JobLog jobLog);
        JobLog GetJobLog(int jobLogId);
        void DeleteJobLog(int jobLogId);
    }
}
