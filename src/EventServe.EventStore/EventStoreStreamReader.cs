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
        private readonly IEventStoreConnection _conn;

        public EventStoreStreamReader(
            IEventStoreConnectionProvider connectionProvider,
            IEventSerializer eventSerializer)
        {
            _connectionProvider = connectionProvider;
            _eventSerializer = eventSerializer;
            _conn = _connectionProvider.GetConnection();
            _conn.ConnectAsync().Wait();
        }

        public async IAsyncEnumerable<Event> ReadAllEventsFromStreamAsync(string stream)
        {
            var credentials = await _connectionProvider.GetCredentials();

            long position = 0;
            var slice = default(StreamEventsSlice);
            do
            {


                slice = await _conn.ReadStreamEventsForwardAsync(stream, position, 100, false, credentials);
                switch (slice.Status)
                {
                    case SliceReadStatus.StreamDeleted: throw new StreamDeletedException(stream);
                    case SliceReadStatus.StreamNotFound: throw new StreamNotFoundException(stream);
                    default: break;
                }

                foreach (var resolvedEvent in slice.Events)
                {
                    if (resolvedEvent.OriginalStreamId[0] == '$')
                        continue;

                    var eventNumber = resolvedEvent.Event.EventNumber;
                    position = eventNumber;
                    var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
                    yield return @event;
                }

                if (!slice.IsEndOfStream)
                    position += 1;
            }
            while (!slice.IsEndOfStream);
 

            yield break;
        }
    }
}
