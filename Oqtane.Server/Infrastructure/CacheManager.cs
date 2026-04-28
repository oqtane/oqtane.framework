using System;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Oqtane.Infrastructure
{
    public class CacheManager : ICacheManager
    {
        private readonly IFusionCache _cache;

        public CacheManager(IFusionCache cache)
        {
            _cache = cache;
        }

        // synchronous / global methods
        public T GetCache<T>(string key, Func<CancellationToken,T> factory)
        {
            return GetCache(null, key, factory);
        }

        public T GetCache<T>(string key, Func<CancellationToken,T> factory, TimeSpan duration)
        {
            return GetCache(null, key, factory, duration);
        }

        public void RemoveCache(string key)
        {
            RemoveCache(null, key);
        }

        // synchronous / multi-tenant methods
        public T GetCache<T>(Alias alias, string key, Func<CancellationToken, T> factory)
        {
            return _cache.GetOrSet(FormatKey(alias, key),
                ct => factory(ct));
        }

        public T GetCache<T>(Alias alias, string key, Func<CancellationToken, T> factory, TimeSpan duration)
        {
            return _cache.GetOrSet(FormatKey(alias, key),
                ct => factory(ct),
                duration);
        }

        public void RemoveCache(Alias alias, string key)
        {
            _cache.Remove(FormatKey(alias, key));
        }

        // asynchronous / global methods
        public async Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory)
        {
            return await GetCacheAsync(null, key, factory);
        }

        public async Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan duration)
        {
            return await GetCacheAsync(null, key, factory, duration);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await RemoveCacheAsync(null, key);
        }

        // asynchronous / multi-tenant methods
        public async Task<T> GetCacheAsync<T>(Alias alias, string key, Func<CancellationToken, Task<T>> factory)
        {
            return await _cache.GetOrSetAsync(FormatKey(alias, key),
                async ct => await factory(ct));
        }

        public async Task<T> GetCacheAsync<T>(Alias alias, string key, Func<CancellationToken, Task<T>> factory, TimeSpan duration)
        {
            return await _cache.GetOrSetAsync(FormatKey(alias, key),
                async ct => await factory(ct),
                duration);
        }

        public async Task RemoveCacheAsync(Alias alias, string key)
        {
            await _cache.RemoveAsync(FormatKey(alias, key));
        }

        private string FormatKey(Alias alias, string key)
        {
            if (alias != null)
            {
                key = $"sitekey:{alias.SiteKey}:{key}";
            }
            return key;
        }
    }
}
