using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IJobRepository
    {
        IEnumerable<Job> GetJobs();
        Job AddJob(Job job);
        Job UpdateJob(Job job);
        Job GetJob(string jobType);
        Job GetJob(int jobId);
        Job GetJob(int jobId, bool tracking);
        void DeleteJob(int jobId);
    }

    public class JobRepository : IJobRepository
    {
        private MasterDBContext _db;
        private readonly ICacheManager _cache;

        public JobRepository(MasterDBContext context, ICacheManager cache)
        {
            _db = context;
            _cache = cache;
        }

        public IEnumerable<Job> GetJobs()
        {
            // remove any jobs which have been uninstalled
            foreach (var job in _db.Job.ToList())
            {
                if (Type.GetType(job.JobType) == null)
                {
                    DeleteJob(job.JobId);
                }
            }
            return _db.Job.ToList();
        }

        public Job AddJob(Job job)
        {
            _db.Job.Add(job);
            _db.SaveChanges();
            return job;
        }

        public Job UpdateJob(Job job)
        {
            _db.Entry(job).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.RemoveCache($"Job:{job.JobType}");
            return job;
        }

        public Job GetJob(string jobType)
        {
            return _cache.GetCache($"Job:{jobType}", entry =>
            {
                return _db.Job.Where(item => item.JobType == jobType).FirstOrDefault();
            });
        }

        public Job GetJob(int jobId)
        {
            return _db.Job.Find(jobId);
        }

        public Job GetJob(int jobId, bool tracking)
        {
            if (tracking)
            {
                return _db.Job.Find(jobId);
            }
            else
            {
                return _db.Job.AsNoTracking().FirstOrDefault(item => item.JobId == jobId);
            }

        }

        public void DeleteJob(int jobId)
        {
            Job job = _db.Job.Find(jobId);
            _db.Job.Remove(job);
            _db.SaveChanges();
            _cache.RemoveCache($"Job:{job.JobType}");
        }
    }
}
