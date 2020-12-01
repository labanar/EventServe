using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventServe.EventStore.Interfaces;
using EventServe.Services;
using EventStore.ClientAPI;
using ES = EventStore.ClientAPI;

namespace EventServe.EventStore
{
    //TODO - implement IDisposable
    public class EventStoreStreamWriter : IEventStreamWriter, IDisposable
    {
        private readonly IEventStoreConnectionProvider _connectionProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventStoreConnection _conn;

        public EventStoreStreamWriter(
            IEventStoreConnectionProvider connectionProvider,
            IEventSerializer eventSerializer)
        {
            _connectionProvider = connectionProvider;
            _eventSerializer = eventSerializer;
            _conn = _connectionProvider.GetConnection();
            _conn.ConnectAsync().Wait();
        }

        public Task AppendEventToStream(string stream, Event @event)
        {
            return AppendEvent(stream, @event, ExpectedVersion.Any);
        }

        public Task AppendEventToStream(string stream, Event @event, long? expectedVersion)
        {
            return AppendEvent(stream, @event, expectedVersion == null ? ExpectedVersion.EmptyStream : expectedVersion.Value);
        }

        public Task AppendEventsToStream(string stream, List<Event> events)
        {
            return AppendEvents(stream, events, ExpectedVersion.Any);
        }

        public Task AppendEventsToStream(string stream, List<Event> events, long? expectedVersion)
        {
            return AppendEvents(stream, events, expectedVersion == null ? ExpectedVersion.EmptyStream : expectedVersion.Value);
        }

        private async Task AppendEvent(string stream, Event @event, long expectedVersion)
        {
            var eventData = _eventSerializer.SerializeEvent(@event);    
            try
            {
                await _conn.AppendToStreamAsync(stream, expectedVersion, eventData);
            }
            catch (ES.Exceptions.WrongExpectedVersionException wrongVersionException)
            {
                throw new WrongExpectedVersionException($"Stream version does not match the expected version {expectedVersion}.");
            }
         }

        private async Task AppendEvents(string stream, List<Event> events, long expectedVersion)
        {
            var eventDatas = new List<EventData>();
            foreach (var @event in events)
                eventDatas.Add(_eventSerializer.SerializeEvent(@event));

            try
            {
                await _conn.AppendToStreamAsync(stream, expectedVersion, eventDatas);
            }
            catch (ES.Exceptions.WrongExpectedVersionException wrongVersionException)
            {
                throw new WrongExpectedVersionException($"Stream version does not match the expected version {expectedVersion}. {wrongVersionException.Message}");
            }
        }

        public void Dispose()
        {
            _conn.Dispose(); 
        }
    }
}