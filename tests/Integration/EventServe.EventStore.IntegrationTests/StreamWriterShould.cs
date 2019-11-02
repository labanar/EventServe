
using EventStore.ClientAPI.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    [Collection("EventStore Collection")]
    public class StreamWriterShould
    {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public StreamWriterShould(EmbeddedEventStoreFixture fixture)
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
        public async Task write_events_to_stream_when_expected_version_matches()
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
                    //new DummyNameChangedEvent(aggregateId, "The new name"),
                    //new DummyUrlChangedEvent(aggregateId, "https://newurl.example.com")
                };

            var sut = new EventStoreStreamWriter(connectionProvider, _serializer);
            await sut.AppendEventsToStream(stream.Id, writeEvents);

            var changeEvent = new DummyUrlChangedEvent(aggregateId, "https://newnewurl.example.com");
            await sut.AppendEventsToStream(stream.Id, new List<Event> { changeEvent }, 0);

        }

        [Fact]
        public async Task throw_on_write_when_expected_version_does_not_match()
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

            //Write 3 events to the stream, this sets the version to 2
            var sut = new EventStoreStreamWriter(connectionProvider, _serializer);
            await sut.AppendEventsToStream(stream.Id, writeEvents);

            //Write should fail
            var changeEvent = new DummyUrlChangedEvent(aggregateId, "https://newnewurl.example.com");
            await Assert.ThrowsAsync<WrongExpectedVersionException>(
                async () => await sut.AppendEventsToStream(stream.Id, new List<Event> { changeEvent }, 1));

        }
    }
}
