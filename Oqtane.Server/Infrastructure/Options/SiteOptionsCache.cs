using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteOptionsCache<TOptions> : IOptionsMonitorCache<TOptions>
        where TOptions : class, new()
    {
        private readonly IAliasAccessor _aliasAccessor;
        private readonly ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>> map = new ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>>();

        public SiteOptionsCache(IAliasAccessor aliasAccessor)
        {
            _aliasAccessor = aliasAccessor;
        }

        public void Clear()
        {
            var cache = map.GetOrAdd(GetKey(), new OptionsCache<TOptions>());
            cache.Clear();
        }

        public void Clear(Alias alias)
        {
            var cache = map.GetOrAdd(alias.SiteKey, new OptionsCache<TOptions>());

            cache.Clear();
        }

        public void ClearAll()
        {
            foreach (var cache in map.Values)
            {
                cache.Clear();
            }
        }

        public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            name = name ?? Options.DefaultName;
            var cache = map.GetOrAdd(GetKey(), new OptionsCache<TOptions>());

            return cache.GetOrAdd(name, createOptions);
        }

        public bool TryAdd(string name, TOptions options)
        {
            name = name ?? Options.DefaultName;
            var cache = map.GetOrAdd(GetKey(), new OptionsCache<TOptions>());

            return cache.TryAdd(name, options);
        }

        public bool TryRemove(string name)
        {
            name = name ?? Options.DefaultName;
            var cache = map.GetOrAdd(GetKey(), new OptionsCache<TOptions>());

            return cache.TryRemove(name);
        }

        private string GetKey()
        {
            return _aliasAccessor?.Alias?.SiteKey ?? "";
        }
    }
}
