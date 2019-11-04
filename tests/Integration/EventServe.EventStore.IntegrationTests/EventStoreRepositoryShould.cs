using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        }

        private EventRepository<T> CreateRepository<T>()
        where T : AggregateRoot {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            var writer = new EventStoreStreamWriter(connectionProvider, _serializer);

            var sut = new EventRepository<T>(writer, reader, null);

            return sut;
        }

    }
}