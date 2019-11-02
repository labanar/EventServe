
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    public class EventStoreShould
    {
        private readonly IOptions<EventStoreConnectionOptions> _eventStoreOptions;
        private readonly EventStoreConnectionProvider _connectionProvider;
        private readonly EventSerializer _serializer;

        public EventStoreShould()
        {
            var random = new Random();
            _eventStoreOptions = Options.Create(new EventStoreConnectionOptions
            {
                Host = "localhost",
                Port = 1113,
                Username = "admin",
                Password = "changeit"
            });

            _connectionProvider = new EventStoreConnectionProvider(_eventStoreOptions);
            _serializer = new EventSerializer();

        }


        [Fact]
        public async Task Write_and_read_events_from_stream()
        {
            var fixture = new EmbeddedEventStoreFixture();
            await fixture.InitializeAsync();

            try
            {
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

                var sut = new EventStoreStreamWriter(_connectionProvider, _serializer);
                await sut.AppendEventsToStream(stream.Id, writeEvents);


                var reader = new EventStoreStreamReader(_connectionProvider, _serializer);
                var readEvents = await reader.ReadAllEventsFromStream(stream.Id);
                readEvents.Count.Should().Be(3);
            }
            finally
            {
                await fixture.DisposeAsync();
            }
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()
        {
            var fixture = new EmbeddedEventStoreFixture();
            await fixture.InitializeAsync();

            try
            {
                var aggregateId = Guid.NewGuid();
                var stream =
                    StreamBuilder.Create()
                    .WithAggregateId(aggregateId)
                    .WithAggregateType<DummyAggregate>()
                    .Build();

                var reader = new EventStoreStreamReader(_connectionProvider, _serializer);
                await Assert.ThrowsAsync<StreamNotFoundException>(async () => await reader.ReadAllEventsFromStream(stream.Id));

            }
            finally
            {
                await fixture.DisposeAsync();
            }
        }
    }
}
