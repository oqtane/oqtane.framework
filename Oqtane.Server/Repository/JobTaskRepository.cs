using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobTaskRepository
    {
        IEnumerable<JobTask> GetJobTasks(int siteId);
        JobTask GetJobTask(int jobTaskId);
        JobTask AddJobTask(JobTask jobTask);
        JobTask UpdateJobTask(JobTask jobTask);
        void DeleteJobTask(int jobTaskId);
    }

    public class JobTaskRepository : IJobTaskRepository
    {
        private TenantDBContext _db;

        public JobTaskRepository(TenantDBContext context)
        {
            _db = context;
        }

        public IEnumerable<JobTask> GetJobTasks(int siteId)
        {
            return _db.JobTask.Where(item => item.SiteId == siteId && !item.IsCompleted).OrderBy(item => item.CreatedOn);
        }

        public JobTask GetJobTask(int jobTaskId)
        {
            return _db.JobTask.SingleOrDefault(item => item.JobTaskId == jobTaskId);
        }

        public JobTask AddJobTask(JobTask jobTask)
        {
            _db.JobTask.Add(jobTask);
            _db.SaveChanges();
            return jobTask;
        }
        public JobTask UpdateJobTask(JobTask jobTask)
        {
            _db.Entry(jobTask).State = EntityState.Modified;
            _db.SaveChanges();
            return jobTask;
        }

        public void DeleteJobTask(int jobTaskId)
        {
            JobTask jobTask = _db.JobTask.Find(jobTaskId);
            _db.JobTask.Remove(jobTask);
            _db.SaveChanges();
        }
    }
}
