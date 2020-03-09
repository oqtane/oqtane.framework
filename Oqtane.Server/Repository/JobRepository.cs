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

        public Job AddJob(Job Job)
        {
            _db.Job.Add(Job);
            _db.SaveChanges();
            return Job;
        }

        public Job UpdateJob(Job Job)
        {
            _db.Entry(Job).State = EntityState.Modified;
            _db.SaveChanges();
            return Job;
        }

        public Job GetJob(int JobId)
        {
            return _db.Job.Find(JobId);
        }

        public void DeleteJob(int JobId)
        {
            Job Job = _db.Job.Find(JobId);
            _db.Job.Remove(Job);
            _db.SaveChanges();
        }
    }
}
