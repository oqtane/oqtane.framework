using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Extensions
{
    public static class CacheExtensions
    {
        private static string _cachekeys = "cachekeys";

        public static TItem GetOrCreate<TItem>(this IMemoryCache cache, string key, Func<ICacheEntry, TItem> factory, bool track)
        {
            if (!cache.TryGetValue(key, out object result))
            {
                using ICacheEntry entry = cache.CreateEntry(key);
                result = factory(entry);
                entry.Value = result;

                if (track)
                {
                    // track the cache key
                    List<string> cachekeys;
                    if (!cache.TryGetValue(_cachekeys, out cachekeys))
                    {
                        cachekeys = new List<string>();
                    }
                    if (!cachekeys.Contains(key))
                    {
                        cachekeys.Add(key);
                        cache.Set(_cachekeys, cachekeys, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
                    }
                }
            }

            return (TItem)result;
        }

        public static void Remove(this IMemoryCache cache, string key, bool track)
        {
            List<string> cachekeys;

            if (track && key.EndsWith("*"))
            {
                // wildcard cache key removal
                var prefix = key.Substring(0, key.Length - 1);
                if (cache.TryGetValue(_cachekeys, out cachekeys) && cachekeys.Any())
                {
                    for (var i = cachekeys.Count - 1; i >= 0; i--)
                    {
                        if (cachekeys[i].StartsWith(prefix))
                        {
                            cache.Remove(cachekeys[i]);
                            cachekeys.RemoveAt(i);
                        }
                    }
                    cache.Set(_cachekeys, cachekeys, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
                }
            }
            else
            {
                cache.Remove(key);
            }

            // reconcile all tracked cache keys
            if (track && cache.TryGetValue(_cachekeys, out cachekeys) && cachekeys.Any())
            {
                for (var i = cachekeys.Count - 1; i >= 0; i--)
                {
                    if (!cache.TryGetValue(cachekeys[i], out _))
                    {
                        cachekeys.RemoveAt(i);
                    }
                }
                cache.Set(_cachekeys, cachekeys, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            }
        }
    }
}
