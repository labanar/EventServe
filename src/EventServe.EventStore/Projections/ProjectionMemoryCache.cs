
using Microsoft.Extensions.Caching.Memory;
using System;

namespace EventServe.EventStore.Projections
{
    public class ProjectionMemoryCache<T> : IProjectionCache<T>
        where T : Projection, new()
    {
        private readonly TimeSpan CACHE_LIFETIME = TimeSpan.FromMinutes(15);
        private readonly MemoryCache _cache;

        public ProjectionMemoryCache()
        {
            var cacheOptions = new MemoryCacheOptions();
            cacheOptions.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
            _cache = new MemoryCache(cacheOptions);
        }

        public bool TryGetValue(string partitionKey, out T projection)
        {
            return _cache.TryGetValue(partitionKey, out projection);
        }

        public void Set(string partitionKey, T retailerProjection)
        {
            _cache.Set(partitionKey, retailerProjection, CACHE_LIFETIME);
        }
    }
}
