using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EventServe.EventStore.Projections
{
    public class ProjectionProvider<T>: IProjectionProvider<T>
        where T: Projection, new()
    {
        private readonly IEventStoreProjectionStateProvider _stateProvider;
        private readonly IProjectionCache<T> _cache;
        private readonly string _projectionName;

        public ProjectionProvider(
            IEventStoreProjectionStateProvider eventStoreProjectionStateProvider,
            IProjectionCache<T> projectionCache)
        {
            _stateProvider = eventStoreProjectionStateProvider;
            _cache = projectionCache;
            _projectionName = new T().ProjectionName;
        }

        public async Task<T> GetState(bool skipCacheRetrieval = false)
        {
            if (!skipCacheRetrieval)
            {
                if (_cache.TryGetValue("ROOT", out var cachedProjection))
                    return cachedProjection;
            }

            var state = await _stateProvider.GetProjectionState(_projectionName);

            if (string.IsNullOrEmpty(state))
                throw new ArgumentException("State cannot be empty");


            var projection = JsonConvert.DeserializeObject<T>(state);
            _cache.Set("ROOT", projection);
            return projection;
        }

        public async Task<T> GetPartitionState(string partitionKey, bool skipCacheRetrieval = false)
        {
            if (!skipCacheRetrieval)
            {
                if (_cache.TryGetValue(partitionKey, out var cachedProjection))
                    return cachedProjection;
            }

            var state = await _stateProvider.GetProjectionState(_projectionName, partitionKey);

            if (string.IsNullOrEmpty(state))
                throw new ArgumentException("State cannot be empty");


            var projection = JsonConvert.DeserializeObject<T>(state);
            _cache.Set(partitionKey, projection);
            return projection;
        }
    }
}
