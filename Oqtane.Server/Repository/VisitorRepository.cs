using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IVisitorRepository
    {
        IEnumerable<Visitor> GetVisitors(int siteId, DateTime fromDate);
        Visitor AddVisitor(Visitor visitor);
        Visitor UpdateVisitor(Visitor visitor);
        Visitor GetVisitor(int visitorId);
        Visitor GetVisitor(int siteId, string IPAddress);
        void DeleteVisitor(int visitorId);
        int DeleteVisitors(int siteId, int age);
    }

    public class VisitorRepository : IVisitorRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public VisitorRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<Visitor> GetVisitors(int siteId, DateTime fromDate)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Visitor.AsNoTracking()
                .Include(item => item.User) // eager load users
                .Where(item => item.SiteId == siteId && item.VisitedOn >= fromDate).ToList();
        }

        public Visitor AddVisitor(Visitor visitor)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Visitor.Add(visitor);
            db.SaveChanges();
            return visitor;
        }

        public Visitor UpdateVisitor(Visitor visitor)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(visitor).State = EntityState.Modified;
            db.SaveChanges();
            return visitor;
        }

        public Visitor GetVisitor(int visitorId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Visitor.Find(visitorId);
        }

        public Visitor GetVisitor(int siteId, string IPAddress)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Visitor.FirstOrDefault(item => item.SiteId == siteId && item.IPAddress == IPAddress);
        }

        public void DeleteVisitor(int visitorId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var visitor = db.Visitor.Find(visitorId);
            db.Visitor.Remove(visitor);
            db.SaveChanges();
        }

        public int DeleteVisitors(int siteId, int age)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete visitors in batches of 100 records
            var count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var visitors = db.Visitor.Where(item => item.SiteId == siteId && item.VisitedOn < purgedate)
                .OrderBy(item => item.VisitedOn).Take(100).ToList();
            while (visitors.Count > 0)
            {
                count += visitors.Count;
                db.Visitor.RemoveRange(visitors);
                db.SaveChanges();
                visitors = db.Visitor.Where(item => item.SiteId == siteId && item.VisitedOn < purgedate)
                    .OrderBy(item => item.VisitedOn).Take(100).ToList();
            }
            return count;
        }
    }
}
