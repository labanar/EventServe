using EventServe.Services;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore
{
    public class SqlStreamStoreStreamReader : IEventStreamReader
    {
        private readonly ISqlStreamStoreProvider _streamStoreProvider;
        private readonly IEventSerializer _eventSerializer;
        private const int PAGE_SIZE = 128;

        public SqlStreamStoreStreamReader(ISqlStreamStoreProvider streamStoreProvider, IEventSerializer eventSerializer)
        {
            _streamStoreProvider = streamStoreProvider;
            _eventSerializer = eventSerializer;
        }

        public async IAsyncEnumerable<Event> ReadAllEventsFromStreamAsync(string stream)
        {
            using var store = await _streamStoreProvider.GetStreamStore();
            var pos = 0;
            var end = false;
            var streamId = new StreamId(stream);
            var page = await store.ReadStreamForwards(streamId.Id, 0, PAGE_SIZE);

            if (page.Status == PageReadStatus.StreamNotFound)
                throw new StreamNotFoundException(stream);

            while (!end)
            {
                //process page results
                var serializationTasks = new List<Task<Event>>(page.Messages.Length);
                foreach (var message in page.Messages)
                    serializationTasks.Add(_eventSerializer.DeseralizeEvent(message));

                await Task.WhenAll(serializationTasks);

                foreach (var task in serializationTasks)
                    yield return task.Result;

                //Check next page
                if (!page.IsEnd)
                {
                    pos += PAGE_SIZE;
                    page = await store.ReadStreamForwards(streamId.Id, pos, PAGE_SIZE);
                }
                else
                {
                    end = true;
                }
            }

            yield break;
        }
    }
}
