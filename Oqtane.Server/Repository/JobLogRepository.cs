using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public class JobLogRepository : IJobLogRepository
    {
        private MasterDBContext db;

        public JobLogRepository(MasterDBContext context)
        {
            db = context;
        }

        public IEnumerable<JobLog> GetJobLogs()
        {
            return db.JobLog
                .Include(item => item.Job) // eager load jobs
                .ToList();
        }

        public JobLog AddJobLog(JobLog JobLog)
        {
            db.JobLog.Add(JobLog);
            db.SaveChanges();
            return JobLog;
        }

        public JobLog UpdateJobLog(JobLog JobLog)
        {
            db.Entry(JobLog).State = EntityState.Modified;
            db.SaveChanges();
            return JobLog;
        }

        public JobLog GetJobLog(int JobLogId)
        {
            return db.JobLog.Include(item => item.Job) // eager load job
                .SingleOrDefault(item => item.JobLogId == JobLogId); 
        }

        public void DeleteJobLog(int JobLogId)
        {
            JobLog Joblog = db.JobLog.Find(JobLogId);
            db.JobLog.Remove(Joblog);
            db.SaveChanges();
        }
    }
}
