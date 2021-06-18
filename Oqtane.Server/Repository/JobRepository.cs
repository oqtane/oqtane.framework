using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class JobRepository : IJobRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;

        public JobRepository(MasterDBContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IEnumerable<Job> GetJobs()
        {
            return _cache.GetOrCreate("jobs", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return _db.Job.ToList();
            });
        }

        public Job AddJob(Job job)
        {
            _db.Job.Add(job);
            _db.SaveChanges();
            _cache.Remove("jobs");
            return job;
        }

        public Job UpdateJob(Job job)
        {
            _db.Entry(job).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("jobs");
            return job;
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
            _cache.Remove("jobs");
        }
    }
}
