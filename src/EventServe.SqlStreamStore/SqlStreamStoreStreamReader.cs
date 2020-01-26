﻿using EventServe.Services;
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
        private const int PAGE_SIZE = 1000;

        public SqlStreamStoreStreamReader(ISqlStreamStoreProvider streamStoreProvider, IEventSerializer eventSerializer)
        {
            _streamStoreProvider = streamStoreProvider;
            _eventSerializer = eventSerializer;
        }

        public async Task<List<Event>> ReadAllEventsFromStream(string stream)
        {
            var store = await _streamStoreProvider.GetStreamStore();

            var pos = 0;
            var end = false;
            var streamId = new StreamId(stream);
            var page = await store.ReadStreamForwards(streamId, 0, PAGE_SIZE);

            if (page.Status == PageReadStatus.StreamNotFound)
                throw new StreamNotFoundException(stream);

            var events = new List<Event>();
            while(!end)
            {
                //process page results
                foreach (var message in page.Messages)
                    events.Add(await _eventSerializer.DeseralizeEvent(message));

                //Check next page
                if(!page.IsEnd)
                {
                    pos += PAGE_SIZE;
                    page = await store.ReadStreamForwards(streamId, pos, PAGE_SIZE);
                }
                else
                {
                    end = true;
                }
            }

            return events;
        }
    }
}
