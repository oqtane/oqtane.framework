using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;
        private readonly DataProtector _dataProtector;

        public TenantRepository(MasterDBContext context, IMemoryCache cache, DataProtector dataProtector)
        {
            _db = context;
            _cache = cache;
            _dataProtector = dataProtector;
        }

        public IEnumerable<Tenant> GetTenants()
        {
            return _cache.GetOrCreate("tenants", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                // Unprotect the ConnectionString after retrieving
                var tenants = _db.Tenant.ToList();
                for(var i = 0; i < tenants.Count; i++)
                {
                    // Unprotect the ConnectionString after retrieving
                    tenants[i].DBConnectionString = _dataProtector.Unprotect(tenants[i].DBConnectionString);
                }

                return tenants;
            });
        }

        public Tenant AddTenant(Tenant tenant)
        {
            // Protect the ConnectionString before persistence
            tenant.DBConnectionString = _dataProtector.Protect(tenant.DBConnectionString);

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

            // Protect the ConnectionString before persistence
            tenant.DBConnectionString = _dataProtector.Protect(tenant.DBConnectionString);

            _db.Entry(tenant).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("tenants");

            return tenant;
        }

        public Tenant GetTenant(int tenantId)
        {
            var tenant = _db.Tenant.Find(tenantId);
            if (tenant != null)
            {
                // Unprotect the ConnectionString after retrieving
                tenant.DBConnectionString = _dataProtector.Unprotect(tenant.DBConnectionString);
            }

            return tenant;
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
