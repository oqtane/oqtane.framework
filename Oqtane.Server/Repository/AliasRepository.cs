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
        private MasterDBContext db;
        private readonly IMemoryCache _cache;

        public AliasRepository(MasterDBContext context, IMemoryCache cache)
        {
            db = context;
            _cache = cache;
        }

        public IEnumerable<Alias> GetAliases()
        {
            return _cache.GetOrCreate("aliases", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return db.Alias.ToList();
            });
        }

        public Alias AddAlias(Alias Alias)
        {
            db.Alias.Add(Alias);
            db.SaveChanges();
            _cache.Remove("aliases");
            return Alias;
        }

        public Alias UpdateAlias(Alias Alias)
        {
            db.Entry(Alias).State = EntityState.Modified;
            db.SaveChanges();
            _cache.Remove("aliases");
            return Alias;
        }

        public Alias GetAlias(int AliasId)
        {
            return db.Alias.Find(AliasId);
        }

        public void DeleteAlias(int AliasId)
        {
            Alias alias = db.Alias.Find(AliasId);
            db.Alias.Remove(alias);
            _cache.Remove("aliases");
            db.SaveChanges();
        }
    }
}
