using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;
using ZiggyCreatures.Caching.Fusion;

namespace Oqtane.Repository
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetTenants();
        Tenant AddTenant(Tenant tenant);
        Tenant UpdateTenant(Tenant tenant);
        Tenant GetTenant(int tenantId);
        void DeleteTenant(int tenantId);
    }
    public class TenantRepository : ITenantRepository
    {
        private MasterDBContext _db;
        private readonly IFusionCache _cache;

        public TenantRepository(MasterDBContext context, IFusionCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IEnumerable<Tenant> GetTenants()
        {
            return _cache.GetOrSet("tenants", entry =>
            {
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
            return GetTenants().FirstOrDefault(item => item.TenantId == tenantId);
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
