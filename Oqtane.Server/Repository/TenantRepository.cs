using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Oqtane.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private HostContext db;
        private readonly IMemoryCache _cache;
        private readonly string alias;

        public TenantRepository(HostContext context, IMemoryCache cache, IHttpContextAccessor accessor)
        {
            db = context;
            _cache = cache;

            // get site alias based on request context
            alias = accessor.HttpContext.Request.Host.Value;
            string path = accessor.HttpContext.Request.Path.Value;
            if (path.StartsWith("/~") && !path.StartsWith("/~/"))
            {
                alias += path.Substring(0, path.IndexOf("/", 1));
            }
        }

        public Tenant GetTenant()
        {
            try
            {
                IEnumerable<Tenant> tenants = _cache.GetOrCreate("tenants", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return db.Tenant.ToList();
                });
                Tenant tenant;
                if (tenants.Count() == 1)
                {
                    tenant = tenants.FirstOrDefault();
                }
                else
                {
                    tenant = tenants.Where(item => item.Alias == alias).FirstOrDefault();
                }
                return tenant;
            }
            catch
            {
                throw;
            }
        }
    }
}
