using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobLogRepository
    {
        IEnumerable<JobLog> GetJobLogs();
        JobLog AddJobLog(JobLog JobLog);
        JobLog UpdateJobLog(JobLog JobLog);
        JobLog GetJobLog(int JobLogId);
        void DeleteJobLog(int JobLogId);
    }
}
