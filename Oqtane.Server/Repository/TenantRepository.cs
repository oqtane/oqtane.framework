using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Shared;

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

        public Tenant AddTenant(Tenant tenant)
        {
            _db.Tenant.Add(tenant);
            _db.SaveChanges();
            _cache.Remove("tenants");
            return tenant;
        }

        public Tenant UpdateTenant(Tenant tenant)
        {
            var oldTenant =_db.Tenant.AsNoTracking().FirstOrDefault(t=> t.TenantId == tenant.TenantId);
            
            if (oldTenant != null && (oldTenant.Name.Equals(TenantNames.Master, StringComparison.OrdinalIgnoreCase) && !oldTenant.Name.Equals(tenant.Name)))
            {
                throw new InvalidOperationException("Unable to rename the master tenant.");
            }
            
            _db.Entry(tenant).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("tenants");
            return tenant;
        }

        public Tenant GetTenant(int tenantId)
        {
            return _db.Tenant.Find(tenantId);
        }

        public void DeleteTenant(int tenantId)
        {
            var tenant = GetTenant(tenantId);
            if (tenant != null && !tenant.Name.Equals(TenantNames.Master, StringComparison.OrdinalIgnoreCase))
            {
                _db.Tenant.Remove(tenant);
                _db.SaveChanges();
            }

            _cache.Remove("tenants");
        }
    }
}
