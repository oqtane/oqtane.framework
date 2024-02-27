using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    TrackCacheKey(cache, key);
                }
            }

            return (TItem)result;
        }

        public static async Task<TItem> GetOrCreateAsync<TItem>(this IMemoryCache cache, string key, Func<ICacheEntry, Task<TItem>> factory, bool track)
        {
            if (!cache.TryGetValue(key, out object result))
            {
                using ICacheEntry entry = cache.CreateEntry(key);
                result = await factory(entry).ConfigureAwait(false);
                entry.Value = result;

                if (track)
                {
                    TrackCacheKey(cache, key);
                }
            }

            return (TItem)result;
        }

        private static void TrackCacheKey(IMemoryCache cache, string key)
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
