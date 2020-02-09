using EventServe.SqlStreamStore.Subscriptions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.SqlStreamStore.IntegrationTests
{
    public class PersistentSubscriptionShould
    {
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly InMemorySqlStreamStoreProvider _storeProvider;

        public PersistentSubscriptionShould()
        {
            _storeProvider = new InMemorySqlStreamStoreProvider();
            _serializer = new SqlStreamStoreEventSerializer();
        }

        [Fact]
        public async Task test_stuff()
        {
            var aggregateId = Guid.NewGuid();
            var streamId =
                StreamIdBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var writeEvents = new List<Event> {
                new DummyCreatedEvent(aggregateId, "The name", "https://url.example.com"),
                new DummyNameChangedEvent(aggregateId, "The new name"),
                new DummyUrlChangedEvent(aggregateId, "https://newurl.example.com")
            };

            var sut = new SqlStreamStoreStreamWriter(_storeProvider, _serializer);
            await sut.AppendEventsToStream(streamId, writeEvents);

            var reader = new SqlStreamStoreStreamReader(_storeProvider, _serializer);

            var count = 0;
            var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
                count++;
            await enumerator.DisposeAsync();
        }
    }
}
