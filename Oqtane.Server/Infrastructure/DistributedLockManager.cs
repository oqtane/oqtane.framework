using System;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Shared;
using ZiggyCreatures.Caching.Fusion;

namespace Oqtane.Infrastructure
{
    public interface IDistributedLockManager
    {
        bool TryAcquireLock(string key, string value, TimeSpan expiration);
        void ReleaseLock(string key);
    }

    public class DistributedLockManager : IDistributedLockManager
    {
        private readonly IFusionCache _cache;
        private readonly IConfigManager _config;

        public DistributedLockManager(IFusionCache cache, IConfigManager config)
        {
            _cache = cache;
            _config = config;
        }

        public bool TryAcquireLock(string key, string value, TimeSpan expiration)
        {
            // check if scale out is enabled and a distributed cache connection string is configured
            if (_config.GetSetting("Caching:ScaleOut", "false") != "true" || string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.DistributedCacheKey)))
            {
                return true;
            }

            // random delay to prevent instances from starting up concurrently (ie. thundering herd)
            int randomDelay = new Random().Next(0, 2000);
            Task.Delay(randomDelay);

            // attempt to get the distributed cache entry
            var cacheEntry = _cache.TryGet<string>(key, new()
            {
                SkipMemoryCacheRead = true,
                SkipMemoryCacheWrite = true,
                IsFailSafeEnabled = false
            });

            if (cacheEntry.HasValue)
            {
                // entry exists - an instance is executing
                return false;
            }

            // entry does not exist - try to create it
            var options = new FusionCacheEntryOptions()
            {
                SkipMemoryCacheRead = true,
                SkipMemoryCacheWrite = true,
                IsFailSafeEnabled = false,
                Duration = expiration,
                DistributedCacheDuration = expiration,
            };
            try
            {
                _cache.Set(key, value, options);
                return true;
            }
            catch
            {
                // failed (likely because another instance acquired it concurrently)
                return false;
            }
        }

        public void ReleaseLock(string key)
        {
            if (!string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.DistributedCacheKey)))
            {
                _cache.Remove(key);
            }
        }
    }
}
