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
                return _cache.GetOrCreate("tenants", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return db.Tenant.ToList();
                });
            }
            catch
            {
                throw;
            }
        }

        public Tenant AddTenant(Tenant Tenant)
        {
            try
            {
                db.Tenant.Add(Tenant);
                db.SaveChanges();
                return Tenant;
            }
            catch
            {
                throw;
            }
        }

        public Tenant UpdateTenant(Tenant Tenant)
        {
            try
            {
                db.Entry(Tenant).State = EntityState.Modified;
                db.SaveChanges();
                return Tenant;
            }
            catch
            {
                throw;
            }
        }

        public Tenant GetTenant(int TenantId)
        {
            try
            {
                return db.Tenant.Find(TenantId);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteTenant(int TenantId)
        {
            try
            {
                Tenant tenant = db.Tenant.Find(TenantId);
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
