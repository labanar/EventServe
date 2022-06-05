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

        public async Task AppendEventsToStream<T>(string stream, List<T> events)
            where T : Event
        {
            var streamId = new StreamId(stream);

            var serializedEvents = new List<NewStreamMessage>();
            foreach (var @event in events)
                serializedEvents.Add(await _eventSerializer.SerializeEvent(@event));

            var store = await _streamStoreProvider.GetStreamStore();
            await store.AppendToStream(streamId.Id, ExpectedVersion.Any, serializedEvents.ToArray());
        }


        public async Task AppendEventsToStream<T>(string stream, List<T> events, long? expectedVersion)
            where T : Event
        {
            var streamId = new StreamId(stream);

            var serializedEvents = new List<NewStreamMessage>();
            foreach (var @event in events)
                serializedEvents.Add(await _eventSerializer.SerializeEvent(@event));

            try
            {
                using var store = await _streamStoreProvider.GetStreamStore();
                await store.AppendToStream(streamId.Id, expectedVersion.HasValue ? (int)expectedVersion.Value : ExpectedVersion.NoStream, serializedEvents.ToArray());
            }
            catch(WrongVersion wV)
            {
                throw new WrongExpectedVersionException(wV.Message, wV);
            }

        }

        public async Task AppendEventToStream<T>(string stream, T @event) where T : Event
        {
            var streamId = new StreamId(stream);
            using var store = await _streamStoreProvider.GetStreamStore();
            await store.AppendToStream(streamId.Id, ExpectedVersion.Any, new NewStreamMessage[] {  await _eventSerializer.SerializeEvent(@event) });
        }

        public async Task AppendEventToStream<T>(string stream, T @event, long? expectedVersion) where T : Event
        {
            var streamId = new StreamId(stream);

            try
            {
                using var store = await _streamStoreProvider.GetStreamStore();
                await store.AppendToStream(streamId.Id, expectedVersion.HasValue ? (int)expectedVersion.Value : ExpectedVersion.NoStream, new NewStreamMessage[] { await _eventSerializer.SerializeEvent(@event) });
            }
            catch (WrongVersion wV)
            {
                throw new WrongExpectedVersionException(wV.Message, wV);
            }
        }
    }
}
