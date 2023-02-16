using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Shared;

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
            return GetAlias(aliasId, true);
        }

        public Alias GetAlias(int aliasId, bool tracking)
        {
            if (tracking)
            {
                return _db.Alias.Find(aliasId);
            }
            else
            {
                return _db.Alias.AsNoTracking().FirstOrDefault(item => item.AliasId == aliasId);
            }
        }

        // lookup alias based on url - note that alias values are hierarchical
        public Alias GetAlias(string url)
        {
            Alias alias = null;

            List<Alias> aliases = GetAliases().ToList();
            var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // iterate segments to find keywords
            int start = segments.Length;
            for (int i = 0; i < segments.Length; i++)
            {
                if (Constants.ReservedRoutes.Contains(segments[i]) || segments[i] == Constants.ModuleDelimiter)
                {
                    start = i;
                    break;
                }
            }

            // iterate segments in reverse order to find alias match
            for (int i = start; i > 0; i--)
            {
                alias = aliases.Find(item => item.Name.Equals(string.Join("/", segments, 0, i), StringComparison.OrdinalIgnoreCase));
                if (alias != null)
                {
                    break; // found a matching alias
                }
            }

            // auto register alias if there is only a single tenant/site
            if (alias == null && aliases.Select(item => new { item.TenantId, item.SiteId }).Distinct().Count() == 1)
            {
                alias = new Alias();
                alias.TenantId = aliases.First().TenantId;
                alias.SiteId = aliases.First().SiteId;
                alias.Name = segments[0]; // root domain
                alias.IsDefault = false;
                alias = AddAlias(alias);
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
