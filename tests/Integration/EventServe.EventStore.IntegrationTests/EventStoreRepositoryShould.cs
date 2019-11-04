using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventServe.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventServe.EventStore.IntegrationTests {

    [Collection("EventStore Collection")]
    public class EventStoreRepositoryShould {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public EventStoreRepositoryShould(EmbeddedEventStoreFixture fixture) {
            _serializer = new EventSerializer();
            _fixture = fixture;
        }

        [Fact]
        public async Task Writes_aggregate_to_stream() {

            var resetCommand = new ResetDummyAggregateCommand() {
                Id = Guid.NewGuid(),
                Name = "The name",
                Url = "https://url.example.com"
            };

            var aggregate = new DummyAggregate(resetCommand);

            var sut = CreateRepository<DummyAggregate>();
            var numberOfChanges = await sut.SaveAsync(aggregate);

            var reader = CreateStreamReader();
            var stream = StreamBuilder.Create()
                .WithAggregateType<DummyAggregate>()
                .WithAggregateId(resetCommand.Id)
                .Build();

            aggregate.Version.Should().Be(0);

            var events = await reader.ReadAllEventsFromStream(stream.Id);
            events.Count.Should().Be(1);
        }

        private EventRepository<T> CreateRepository<T>()
        where T : AggregateRoot {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            var writer = new EventStoreStreamWriter(connectionProvider, _serializer);

            var sut = new EventRepository<T>(writer, reader);

            return sut;
        }

        private IEventStreamReader CreateStreamReader() {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));
            return new EventStoreStreamReader(connectionProvider, _serializer);
        }

    }
}