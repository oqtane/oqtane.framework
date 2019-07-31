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
            try
            {
                IEnumerable<Tenant> tenants = _cache.GetOrCreate("tenants", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return db.Tenant.ToList();
                });
                return tenants;
            }
            catch
            {
                throw;
            }
        }

        public void AddTenant(Tenant tenant)
        {
            try
            {
                db.Tenant.Add(tenant);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateTenant(Tenant tenant)
        {
            try
            {
                db.Entry(tenant).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Tenant GetTenant(int tenantId)
        {
            try
            {
                Tenant tenant = db.Tenant.Find(tenantId);
                return tenant;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteTenant(int tenantId)
        {
            try
            {
                Tenant tenant = db.Tenant.Find(tenantId);
                db.Tenant.Remove(tenant);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
