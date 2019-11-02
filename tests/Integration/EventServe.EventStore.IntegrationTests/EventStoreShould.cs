
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    public class EventStoreShould: IClassFixture<EmbeddedEventStoreFixture>
    {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public EventStoreShould(EmbeddedEventStoreFixture fixture)
        {
            _serializer = new EventSerializer();
            _fixture = fixture;
        }

        [Fact]
        public async Task Write_and_read_events_from_stream()
        {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var aggregateId = Guid.NewGuid();
            var stream =
                StreamBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var writeEvents = new List<Event>
                {
                    new DummyCreatedEvent(aggregateId, "The name", "https://url.example.com"),
                    new DummyNameChangedEvent(aggregateId, "The new name"),
                    new DummyUrlChangedEvent(aggregateId, "https://newurl.example.com")
                };

            var sut = new EventStoreStreamWriter(connectionProvider, _serializer);
            await sut.AppendEventsToStream(stream.Id, writeEvents);


            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            var readEvents = await reader.ReadAllEventsFromStream(stream.Id);
            readEvents.Count.Should().Be(3);
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()
        {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var aggregateId = Guid.NewGuid();
            var stream =
                StreamBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            await Assert.ThrowsAsync<StreamNotFoundException>(async () => await reader.ReadAllEventsFromStream(stream.Id));
        }
    }
}
