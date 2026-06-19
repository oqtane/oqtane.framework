using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public interface IDistributedLockManager
    {
        bool TryAcquireLock(string key, string value, TimeSpan expiration);
        void ReleaseLock(string key);
    }

    public class DistributedLockManager : IDistributedLockManager
    {
        private readonly IDistributedCache _cache;
        private readonly IConfigManager _config;

        public DistributedLockManager(IDistributedCache cache, IConfigManager config)
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
            var bytes = _cache.Get(key);

            if (bytes != null)
            {
                // entry exists - an instance is executing
                return false;
            }

            // entry does not exist - try to create it
            bytes = Encoding.UTF8.GetBytes(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            try
            {
                _cache.Set(key, bytes, options);
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
