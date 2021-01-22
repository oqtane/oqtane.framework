using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;

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

        public Alias AddAlias(Alias alias)
        {
            _db.Alias.Add(alias);
            _db.SaveChanges();
            _cache.Remove("aliases");
            return alias;
        }

        public Alias UpdateAlias(Alias alias)
        {
            _db.Entry(alias).State = EntityState.Modified;
            _db.SaveChanges();
            _cache.Remove("aliases");
            return alias;
        }

        public Alias GetAlias(int aliasId)
        {
            return _db.Alias.Find(aliasId);
        }

        public Alias GetAlias(string name)
        {
            Alias alias = null;

            List<Alias> aliases = GetAliases().ToList();
            var segments = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // iterate segments in reverse order
            for (int i = segments.Length; i > 0; i--)
            {
                name = string.Join("/", segments, 0, i);
                alias = aliases.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (alias != null)
                {
                    break; // found a matching alias
                }
            }

            return alias;
        }

        public void DeleteAlias(int aliasId)
        {
            Alias alias = _db.Alias.Find(aliasId);
            _db.Alias.Remove(alias);
            _cache.Remove("aliases");
            _db.SaveChanges();
        }
    }
}
