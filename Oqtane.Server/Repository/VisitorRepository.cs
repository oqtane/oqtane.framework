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

        public void DeleteVisitor(int visitorId)
        {
            Visitor visitor = _db.Visitor.Find(visitorId);
            _db.Visitor.Remove(visitor);
            _db.SaveChanges();
        }
    }
}
