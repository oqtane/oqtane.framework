using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class TenantRepository : Repository<Tenant>
    {
        private static readonly string _key = "tenant";

        private readonly MasterDBContext _context;
        private readonly IMemoryCache _cache;

        public TenantRepository(MasterDBContext context, IMemoryCache cache)
            : base(context)
        {
            _context = context;
            _cache = cache;
        }

        public override Tenant Add(Tenant entity)
        {
            _cache.Remove(_key);

            return base.Add(entity);
        }

        public override IEnumerable<Tenant> GetAll()
        {
            return _cache.GetOrCreate(_key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);

                return _context.Set<Tenant>().ToList();
            });
        }

        public override void Delete(int id)
        {
            base.Delete(id);
            _cache.Remove(_key);
        }

        public override Tenant Update(Tenant entity)
        {
            _cache.Remove(_key);

            return base.Update(entity);
        }
    }
}
