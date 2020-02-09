﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace EventServe.SqlStreamStore.IntegrationTests
{

    public class StreamWriterShould
    {
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly InMemorySqlStreamStoreProvider _storeProvider;

        public StreamWriterShould()
        {
            _storeProvider = new InMemorySqlStreamStoreProvider();
            _serializer = new SqlStreamStoreEventSerializer();
        }

        [Fact]
        public async Task write_events_to_stream()
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
            count.Should().Be(3);
        }

        [Fact]
        public async Task write_events_to_stream_when_expected_version_matches()
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

            var changeEvent = new DummyUrlChangedEvent(aggregateId, "https://newnewurl.example.com");
            await sut.AppendEventsToStream(streamId, new List<Event> { changeEvent }, 2);
        }

        [Fact]
        public async Task throw_on_write_when_expected_version_does_not_match()
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

            //Write 3 events to the stream, this sets the version to 2
            var sut = new SqlStreamStoreStreamWriter(_storeProvider, _serializer);
            await sut.AppendEventsToStream(streamId, writeEvents);

            //Write should fail
            var changeEvent = new DummyUrlChangedEvent(aggregateId, "https://newnewurl.example.com");
            await Assert.ThrowsAsync<WrongExpectedVersionException>(
                async () => await sut.AppendEventsToStream(streamId, new List<Event> { changeEvent }, 1));

        }
    }
}
