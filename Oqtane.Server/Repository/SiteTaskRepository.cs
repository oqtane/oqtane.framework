using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteTaskRepository
    {
        IEnumerable<SiteTask> GetSiteTasks(int siteId);
        SiteTask GetSiteTask(int siteTaskId);
        SiteTask AddSiteTask(SiteTask siteTask);
        SiteTask UpdateSiteTask(SiteTask siteTask);
        void DeleteSiteTask(int siteTaskId);
        int DeleteSiteTasks(int siteId, int age);
    }

    public class SiteTaskRepository : ISiteTaskRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SiteTaskRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<SiteTask> GetSiteTasks(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteTask.Where(item => item.SiteId == siteId && !item.IsCompleted)
                .OrderBy(item => item.CreatedOn).ToList();
        }

        public SiteTask GetSiteTask(int siteTaskId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteTask.SingleOrDefault(item => item.SiteTaskId == siteTaskId);
        }

        public SiteTask AddSiteTask(SiteTask siteTask)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SiteTask.Add(siteTask);
            db.SaveChanges();
            return siteTask;
        }
        public SiteTask UpdateSiteTask(SiteTask siteTask)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(siteTask).State = EntityState.Modified;
            db.SaveChanges();
            return siteTask;
        }

        public void DeleteSiteTask(int siteTaskId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            SiteTask siteTask = db.SiteTask.Find(siteTaskId);
            db.SiteTask.Remove(siteTask);
            db.SaveChanges();
        }

        public int DeleteSiteTasks(int siteId, int age)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete completed tasks in batches of 100 records
            var count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var tasks = db.SiteTask.Where(item => item.SiteId == siteId && item.IsCompleted && item.CreatedOn < purgedate)
                .OrderBy(item => item.CreatedOn).Take(100).ToList();
            while (tasks.Count > 0)
            {
                count += tasks.Count;
                db.SiteTask.RemoveRange(tasks);
                db.SaveChanges();
                tasks = db.SiteTask.Where(item => item.SiteId == siteId && item.IsCompleted && item.CreatedOn < purgedate)
                    .OrderBy(item => item.CreatedOn).Take(100).ToList();
            }
            return count;
        }
    }
}
