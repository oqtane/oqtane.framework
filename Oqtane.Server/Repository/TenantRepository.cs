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
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;

        public TenantRepository(MasterDBContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IEnumerable<Tenant> GetTenants()
        {
            return _cache.GetOrCreate("tenants", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return _db.Tenant.ToList();
            });
        }

        public Tenant AddTenant(Tenant Tenant)
        {
            _db.Tenant.Add(Tenant);
            _db.SaveChanges();
            _cache.Remove("tenants");
            return Tenant;
        }

        public Tenant UpdateTenant(Tenant Tenant)
        {
            _db.Entry(Tenant).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("tenants");
            return Tenant;
        }

        public Tenant GetTenant(int TenantId)
        {
            return _db.Tenant.Find(TenantId);
        }

        public void DeleteTenant(int TenantId)
        { 
            Tenant tenant = _db.Tenant.Find(TenantId);
            _db.Tenant.Remove(tenant);
            _db.SaveChanges();
            _cache.Remove("tenants");
        }
    }
}
