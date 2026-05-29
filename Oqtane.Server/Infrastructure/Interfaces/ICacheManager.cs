using System;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ICacheManager
    {
        // synchronous / global methods
        T GetCache<T>(string key, Func<CancellationToken, T> factory);
        T GetCache<T>(string key, Func<CancellationToken, T> factory, TimeSpan? memoryCacheDuration, TimeSpan? distributedCacheDuration);
        void RemoveCache(string key);

        // synchronous / multi-tenant methods
        T GetCache<T>(Alias alias, string key, Func<CancellationToken, T> factory);
        T GetCache<T>(Alias alias, string key, Func<CancellationToken, T> factory, TimeSpan? memoryCacheDuration, TimeSpan? distributedCacheDuration);
        void RemoveCache(Alias alias, string key);

        // asynchronous / global methods
        Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory);
        Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? memoryCacheDuration, TimeSpan? distributedCacheDuration);
        Task RemoveCacheAsync(string key);

        // asynchronous / multi-tenant methods
        Task<T> GetCacheAsync<T>(Alias alias, string key, Func<CancellationToken, Task<T>> factory);
        Task<T> GetCacheAsync<T>(Alias alias, string key, Func<CancellationToken, Task<T>> factory, TimeSpan? memoryCacheDuration, TimeSpan? distributedCacheDuration);
        Task RemoveCacheAsync(Alias alias, string key);
    }
}
