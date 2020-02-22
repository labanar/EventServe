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
            var slices = default(StreamEventsSlice);
            do
            {
                if (position != 0)
                    position += 1;

                slices = await _conn.ReadStreamEventsForwardAsync(stream, position, 4096, false, credentials);
                switch (slices.Status)
                {
                    case SliceReadStatus.StreamDeleted: throw new StreamDeletedException(stream);
                    case SliceReadStatus.StreamNotFound: throw new StreamNotFoundException(stream);
                    default: break;
                }

                foreach (var resolvedEvent in slices.Events)
                {
                    if (resolvedEvent.OriginalStreamId[0] == '$')
                        continue;

                    position = resolvedEvent.Event.EventNumber;
                    yield return _eventSerializer.DeseralizeEvent(resolvedEvent);
                }
            }
            while (!slices.IsEndOfStream);
 

            yield break;
        }
    }
}
