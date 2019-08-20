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
                return _cache.GetOrCreate("aliases", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return db.Alias.ToList();
                });
            }
            catch
            {
                throw;
            }
        }

        public Alias AddAlias(Alias Alias)
        {
            try
            {
                db.Alias.Add(Alias);
                db.SaveChanges();
                return Alias;
            }
            catch
            {
                throw;
            }
        }

        public Alias UpdateAlias(Alias Alias)
        {
            try
            {
                db.Entry(Alias).State = EntityState.Modified;
                db.SaveChanges();
                return Alias;
            }
            catch
            {
                throw;
            }
        }

        public Alias GetAlias(int AliasId)
        {
            try
            {
                return db.Alias.Find(AliasId);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteAlias(int AliasId)
        {
            try
            {
                Alias alias = db.Alias.Find(AliasId);
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
