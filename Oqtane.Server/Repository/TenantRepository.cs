using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private MasterDBContext db;
        private readonly IMemoryCache _cache;

        public TenantRepository(MasterDBContext context, IMemoryCache cache)
        {
            db = context;
            _cache = cache;
        }

        public IEnumerable<Tenant> GetTenants()
        {
            return _cache.GetOrCreate("tenants", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return db.Tenant.ToList();
            });
        }

        public Tenant AddTenant(Tenant Tenant)
        {
            db.Tenant.Add(Tenant);
            db.SaveChanges();
            _cache.Remove("tenants");
            return Tenant;
        }

        public Tenant UpdateTenant(Tenant Tenant)
        {
            db.Entry(Tenant).State = EntityState.Modified;
            db.SaveChanges();
            _cache.Remove("tenants");
            return Tenant;
        }

        public Tenant GetTenant(int TenantId)
        {
            return db.Tenant.Find(TenantId);
        }

        public void DeleteTenant(int TenantId)
        { 
            Tenant tenant = db.Tenant.Find(TenantId);
            db.Tenant.Remove(tenant);
            db.SaveChanges();
            _cache.Remove("tenants");
        }
    }
}
