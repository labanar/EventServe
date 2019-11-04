using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventServe.EventStore.IntegrationTests {
    [Collection("EventStore Collection")]
    public class StreamReaderShould {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public StreamReaderShould(EmbeddedEventStoreFixture fixture) {
            _serializer = new EventSerializer();
            _fixture = fixture;
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist() {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var aggregateId = Guid.NewGuid();
            var stream =
                StreamBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            await Assert.ThrowsAsync<StreamNotFoundException>(async() => await reader.ReadAllEventsFromStream(stream.Id));
        }
    }
}