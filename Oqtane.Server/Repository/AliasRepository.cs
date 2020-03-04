using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Oqtane.Repository
{
    public class AliasRepository : IAliasRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;

        public AliasRepository(MasterDBContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IEnumerable<Alias> GetAliases()
        {
            return _cache.GetOrCreate("aliases", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return _db.Alias.ToList();
            });
        }

        public Alias AddAlias(Alias Alias)
        {
            _db.Alias.Add(Alias);
            _db.SaveChanges();
            _cache.Remove("aliases");
            return Alias;
        }

        public Alias UpdateAlias(Alias Alias)
        {
            _db.Entry(Alias).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("aliases");
            return Alias;
        }

        public Alias GetAlias(int AliasId)
        {
            return _db.Alias.Find(AliasId);
        }

        public void DeleteAlias(int AliasId)
        {
            Alias alias = _db.Alias.Find(AliasId);
            _db.Alias.Remove(alias);
            _cache.Remove("aliases");
            _db.SaveChanges();
        }
    }
}
