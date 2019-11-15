using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Oqtane.Repository
{
    public class JobRepository : IJobRepository
    {
        private MasterDBContext db;

        public JobRepository(MasterDBContext context)
        {
            db = context;
        }

        public IEnumerable<Job> GetJobs()
        {
            return db.Job.ToList();
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
