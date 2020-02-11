using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace Oqtane.Repository
{
    public class JobRepository : IJobRepository
    {
        private MasterDBContext db;
        private readonly IMemoryCache _cache;

        public JobRepository(MasterDBContext context, IMemoryCache cache)
        {
            db = context;
            _cache = cache;
        }

        public IEnumerable<Job> GetJobs()
        {
            return _cache.GetOrCreate("jobs", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return db.Job.ToList();
            });
        }

        public Job AddJob(Job Job)
        {
            db.Job.Add(Job);
            db.SaveChanges();
            return Job;
        }

        public Job UpdateJob(Job Job)
        {
            db.Entry(Job).State = EntityState.Modified;
            db.SaveChanges();
            return Job;
        }

        public Job GetJob(int JobId)
        {
            return db.Job.Find(JobId);
        }

        public void DeleteJob(int JobId)
        {
            Job Job = db.Job.Find(JobId);
            db.Job.Remove(Job);
            db.SaveChanges();
        }
    }
}
