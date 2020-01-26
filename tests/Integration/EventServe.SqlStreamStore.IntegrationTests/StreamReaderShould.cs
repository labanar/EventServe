using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.SqlStreamStore.IntegrationTests
{
    public class StreamReaderShould
    {
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly InMemorySqlStreamStoreProvider _storeProvider;

        public StreamReaderShould()
        {
            _storeProvider = new InMemorySqlStreamStoreProvider();
            _serializer = new SqlStreamStoreEventSerializer();
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()
        {
            var aggregateId = Guid.NewGuid();
            var stream =
                StreamBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var reader = new SqlStreamStoreStreamReader(_storeProvider, _serializer);
            await Assert.ThrowsAsync<StreamNotFoundException>(async () => await reader.ReadAllEventsFromStream(stream.Id));
        }
    }
}
