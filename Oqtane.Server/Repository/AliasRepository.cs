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
            try
            {
                IEnumerable<Alias> aliases = _cache.GetOrCreate("aliases", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return db.Alias.ToList();
                });
                return aliases;
            }
            catch
            {
                throw;
            }
        }

        public void AddAlias(Alias alias)
        {
            try
            {
                db.Alias.Add(alias);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateAlias(Alias alias)
        {
            try
            {
                db.Entry(alias).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Alias GetAlias(int aliasId)
        {
            try
            {
                Alias alias = db.Alias.Find(aliasId);
                return alias;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteAlias(int aliasId)
        {
            try
            {
                Alias alias = db.Alias.Find(aliasId);
                db.Alias.Remove(alias);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
