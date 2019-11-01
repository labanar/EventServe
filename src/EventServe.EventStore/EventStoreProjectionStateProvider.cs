using EventServe.EventStore.Projections;
using EventStore.ClientAPI.Projections;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using IEventStoreLogger = EventStore.ClientAPI.ILogger;

namespace EventServe.EventStore
{
    public class EventStoreProjectionStateProvider: IEventStoreProjectionStateProvider
    {
        private readonly EventStoreConnectionOptions _options;
        private readonly IEventStoreLogger _logger;

        public EventStoreProjectionStateProvider(
            IOptions<EventStoreConnectionOptions> options,
            IEventStoreLogger logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GetProjectionState(string projectionName)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_options.Host), 2113);
            var manager = new ProjectionsManager(_logger, endpoint, new TimeSpan(0, 0, 30));
            var state = await manager.GetStateAsync(projectionName);
            return state;
        }

        public async Task<string> GetProjectionState(string projectionName, string partitionId)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_options.Host), 2113);
            var manager = new ProjectionsManager(_logger, endpoint, new TimeSpan(0, 0, 30));
            var state = await manager.GetPartitionStateAsync(projectionName, partitionId);
            return state;
        }
    }

    
}
