using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public class JobLogRepository : IJobLogRepository
    {
        private MasterDBContext _db;

        public JobLogRepository(MasterDBContext context)
        {
            _db = context;
        }

        public IEnumerable<JobLog> GetJobLogs()
        {
            return _db.JobLog
                .Include(item => item.Job) // eager load jobs
                .ToList();
        }

        public JobLog AddJobLog(JobLog JobLog)
        {
            _db.JobLog.Add(JobLog);
            _db.SaveChanges();
            return JobLog;
        }

        public JobLog UpdateJobLog(JobLog JobLog)
        {
            _db.Entry(JobLog).State = EntityState.Modified;
            _db.SaveChanges();
            return JobLog;
        }

        public JobLog GetJobLog(int JobLogId)
        {
            return _db.JobLog.Include(item => item.Job) // eager load job
                .SingleOrDefault(item => item.JobLogId == JobLogId); 
        }

        public void DeleteJobLog(int JobLogId)
        {
            JobLog Joblog = _db.JobLog.Find(JobLogId);
            _db.JobLog.Remove(Joblog);
            _db.SaveChanges();
        }
    }
}
