using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class VisitorRepository : IVisitorRepository
    {
        private TenantDBContext _db;

        public VisitorRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<Visitor> GetVisitors(int siteId, DateTime fromDate)
        {
            return _db.Visitor.AsNoTracking()
                .Include(item => item.User) // eager load users
                .Where(item => item.SiteId == siteId && item.VisitedOn >= fromDate);
        }

        public Visitor AddVisitor(Visitor visitor)
        {
            _db.Visitor.Add(visitor);
            _db.SaveChanges();
            return visitor;
        }

        public Visitor UpdateVisitor(Visitor visitor)
        {
            _db.Entry(visitor).State = EntityState.Modified;
            _db.SaveChanges();
            return visitor;
        }

        public Visitor GetVisitor(int visitorId)
        {
            return _db.Visitor.Find(visitorId);
        }

        public Visitor GetVisitor(int siteId, string IPAddress)
        {
            return _db.Visitor.FirstOrDefault(item => item.SiteId == siteId && item.IPAddress == IPAddress);
        }

        public void DeleteVisitor(int visitorId)
        {
            Visitor visitor = _db.Visitor.Find(visitorId);
            _db.Visitor.Remove(visitor);
            _db.SaveChanges();
        }

        public int DeleteVisitors(int siteId, int age)
        {
            // delete visitors in batches of 100 records
            int count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var visitors = _db.Visitor.Where(item => item.SiteId == siteId && item.Visits < 2 && item.VisitedOn < purgedate)
                .OrderBy(item => item.VisitedOn).Take(100).ToList();
            while (visitors.Count > 0)
            {
                count += visitors.Count;
                _db.Visitor.RemoveRange(visitors);
                _db.SaveChanges();
                visitors = _db.Visitor.Where(item => item.SiteId == siteId && item.Visits < 2 && item.VisitedOn < purgedate)
                    .OrderBy(item => item.VisitedOn).Take(100).ToList();
            }
            return count;
        }
    }
}
