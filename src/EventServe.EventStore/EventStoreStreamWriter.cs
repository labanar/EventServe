using System.Collections.Generic;
using System.Threading.Tasks;
using EventServe.EventStore.Interfaces;
using EventServe.Services;
using EventStore.ClientAPI;
using ES = EventStore.ClientAPI;

namespace EventServe.EventStore
{
    public class EventStoreStreamWriter : IEventStreamWriter
    {
        private readonly IEventStoreConnectionProvider _connectionProvider;
        private readonly IEventSerializer _eventSerializer;

        public EventStoreStreamWriter(
            IEventStoreConnectionProvider connectionProvider,
            IEventSerializer eventSerializer)
        {
            _connectionProvider = connectionProvider;
            _eventSerializer = eventSerializer;
        }

        public Task AppendEventToStream<T>(string stream, T @event)
            where T: Event
        {
            return AppendEvent(stream, @event, ExpectedVersion.Any);
        }

        public Task AppendEventToStream<T>(string stream, T @event, long? expectedVersion)
            where T: Event
        {
            return AppendEvent(stream, @event, expectedVersion == null ? ExpectedVersion.NoStream : expectedVersion.Value);
        }

        public Task AppendEventsToStream<T>(string stream, List<T> events)
            where T: Event
        {
            return AppendEvents(stream, events, ExpectedVersion.Any);
        }

        public Task AppendEventsToStream<T>(string stream, List<T> events, long? expectedVersion)
            where T: Event
        {
            return AppendEvents(stream, events, expectedVersion == null ? ExpectedVersion.NoStream : expectedVersion.Value);
        }

        private async Task AppendEvent<T>(string stream, T @event, long expectedVersion)
            where T: Event
        {
            var connection = await _connectionProvider.GetConnection();
            var eventData = _eventSerializer.SerializeEvent(@event);    
            try
            {
                await connection.AppendToStreamAsync(stream, expectedVersion, eventData);
            }
            catch (ES.Exceptions.WrongExpectedVersionException wrongVersionException)
            {
                throw new WrongExpectedVersionException($"Stream version does not match the expected version {expectedVersion}.");
            }
         }

        private async Task AppendEvents<T>(string stream, List<T> events, long expectedVersion)
            where T: Event
        {
            var eventDatas = new List<EventData>();
            foreach (var @event in events)
                eventDatas.Add(_eventSerializer.SerializeEvent(@event));

            var connection = await _connectionProvider.GetConnection();

            try
            {
                await connection.AppendToStreamAsync(stream, expectedVersion, eventDatas);
            }
            catch (ES.Exceptions.WrongExpectedVersionException wrongVersionException)
            {
                throw new WrongExpectedVersionException($"Stream version does not match the expected version {expectedVersion}. {wrongVersionException.Message}");
            }
        }
    }
}