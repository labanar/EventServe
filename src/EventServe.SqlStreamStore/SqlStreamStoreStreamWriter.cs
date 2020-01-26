using EventServe.Services;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WrongVersion = SqlStreamStore.Streams.WrongExpectedVersionException;

namespace EventServe.SqlStreamStore
{
    public class SqlStreamStoreStreamWriter: IEventStreamWriter
    {
        private readonly ISqlStreamStoreProvider _streamStoreProvider;
        private readonly IEventSerializer _eventSerializer;

        public SqlStreamStoreStreamWriter(ISqlStreamStoreProvider streamStoreProvider, IEventSerializer eventSerializer)
        {
            _streamStoreProvider = streamStoreProvider;
            _eventSerializer = eventSerializer;
        }

        public async Task AppendEventsToStream(string stream, List<Event> events)
        {
            var streamId = new StreamId(stream);

            var serializedEvents = new List<NewStreamMessage>();
            foreach (var @event in events)
                serializedEvents.Add(await _eventSerializer.SerializeEvent(@event));

            var store = await _streamStoreProvider.GetStreamStore();
            await store.AppendToStream(streamId, ExpectedVersion.Any, serializedEvents.ToArray());
        }


        public async Task AppendEventsToStream(string stream, List<Event> events, long expectedVersion)
        {
            var streamId = new StreamId(stream);

            var serializedEvents = new List<NewStreamMessage>();
            foreach (var @event in events)
                serializedEvents.Add(await _eventSerializer.SerializeEvent(@event));

            try
            {
                var store = await _streamStoreProvider.GetStreamStore();
                await store.AppendToStream(streamId, (int)expectedVersion, serializedEvents.ToArray());
            }
            catch(WrongVersion wV)
            {
                throw new WrongExpectedVersionException(wV.Message, wV);
            }

        }

        public async Task AppendEventToStream(string stream, Event @event)
        {
            var streamId = new StreamId(stream);
            var store = await _streamStoreProvider.GetStreamStore();
            await store.AppendToStream(streamId, ExpectedVersion.Any, new NewStreamMessage[] {  await _eventSerializer.SerializeEvent(@event) });
        }

        public async Task AppendEventToStream(string stream, Event @event, long expectedVersion)
        {
            var streamId = new StreamId(stream);
            var store = await _streamStoreProvider.GetStreamStore();

            try
            {
                await store.AppendToStream(streamId, (int)expectedVersion, new NewStreamMessage[] { await _eventSerializer.SerializeEvent(@event) });
            }
            catch (WrongVersion wV)
            {
                throw new WrongExpectedVersionException(wV.Message, wV);
            }
        }
    }
}
