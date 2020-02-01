using EventServe.EventStore.Interfaces;
using EventServe.Services;
using EventStore.ClientAPI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public class EventStoreStreamReader : IEventStreamReader
    {
        private readonly IEventStoreConnectionProvider _connectionProvider;
        private readonly IEventSerializer _eventSerializer;

        public EventStoreStreamReader(
            IEventStoreConnectionProvider connectionProvider,
            IEventSerializer eventSerializer)
        {
            _connectionProvider = connectionProvider;
            _eventSerializer = eventSerializer;
        }

        public async Task<List<Event>> ReadAllEventsFromStream(string stream)
        {
            var events = new List<Event>();
            using (var conn = _connectionProvider.GetConnection())
            {
                await conn.ConnectAsync();
                var credentials = await _connectionProvider.GetCredentials();

                long position = 0;
                var slices = default(StreamEventsSlice);
                do
                {
                    slices = await conn.ReadStreamEventsForwardAsync(stream, position, 4096, false, credentials);
                    switch (slices.Status)
                    {
                        case SliceReadStatus.StreamDeleted: throw new StreamDeletedException(stream);
                        case SliceReadStatus.StreamNotFound: throw new StreamNotFoundException(stream);
                        default: break;
                    }

                    foreach (var resolvedEvent in slices.Events)
                    {
                        events.Add(_eventSerializer.DeseralizeEvent(resolvedEvent));
                        position = resolvedEvent.Event.EventNumber;
                    }
                }
                while (!slices.IsEndOfStream);
            }
            return events;
        }
    }
}
