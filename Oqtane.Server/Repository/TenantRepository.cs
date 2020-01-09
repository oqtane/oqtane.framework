using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private readonly MasterDBContext db;
        private readonly IMemoryCache _cache;

        public TenantRepository(MasterDBContext context, IMemoryCache cache)
        {
            db = context;
            _cache = cache;
        }

        public IEnumerable<Tenant> GetAll()
        {
            return _cache.GetOrCreate("tenants", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                
                return db.Tenant.ToList();
            });
        }

        public Tenant Add(Tenant tenant)
        {
            db.Tenant.Add(tenant);
            db.SaveChanges();
            _cache.Remove("tenants");
            
            return tenant;
        }

        public Tenant Update(Tenant tenant)
        {
            db.Entry(tenant).State = EntityState.Modified;
            db.SaveChanges();
            _cache.Remove("tenants");
            
            return tenant;
        }

        public Tenant Get(int id)
        {
            return db.Tenant.Find(id);
        }

        public void Delete(int id)
        { 
            var tenant = db.Tenant.Find(id);
            db.Tenant.Remove(tenant);
            db.SaveChanges();
            _cache.Remove("tenants");
        }
    }
}
