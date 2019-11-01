using EventServe.EventStore.Interfaces;
using EventServe.Services;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public class EventStoreStreamWriter: IEventStreamWriter
    {
        private readonly IEventStoreConnectionProvider _connectionProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly ILogger<EventStoreStreamWriter> _logger;

        public EventStoreStreamWriter(
            IEventStoreConnectionProvider connectionProvider,
            IEventSerializer eventSerializer,
            ILogger<EventStoreStreamWriter> logger)
        {
            _connectionProvider = connectionProvider;
            _eventSerializer = eventSerializer;
            _logger = logger;
        }

        public Task AppendEventToStream(string stream, Event @event)
        {
            return AppendEvent(stream, @event, ExpectedVersion.Any);
        }

        public Task AppendEventToStream(string stream, Event @event, long expectedVersion)
        {
            return AppendEvent(stream, @event, expectedVersion);
        }

        public Task AppendEventsToStream(string stream, List<Event> events)
        {
            return AppendEvents(stream, events, ExpectedVersion.Any);
        }

        public Task AppendEventsToStream(string stream, List<Event> events, long expectedVersion)
        {
            return AppendEvents(stream, events, expectedVersion);
        }

        private async Task AppendEvent(string stream, Event @event, long expectedVersion)
        {
            var eventData = _eventSerializer.SerializeEvent(@event);

            using (var conn = _connectionProvider.GetConnection())
            {
                await conn.ConnectAsync();                
                await conn.AppendToStreamAsync(stream, expectedVersion, eventData);
            }
        }

        private async Task AppendEvents(string stream, List<Event> events, long expectedVersion)
        {
            var eventDatas = new List<EventData>();
            foreach (var @event in events)
                eventDatas.Add(_eventSerializer.SerializeEvent(@event));

            using (var conn = _connectionProvider.GetConnection())
            {
                await conn.ConnectAsync();
                await conn.AppendToStreamAsync(stream, expectedVersion, eventDatas);
            }
        }
    }
}
