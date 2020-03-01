using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    [Collection("SqlStreamStore Collection")]
    public class SqlStreamReaderShould
    {
        private readonly EmbeddedMsSqlStreamStoreFixture _fixture;
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly MsSqlStreamStoreSettingsProvider _settingsProvider;
        private readonly MsSqlStreamStoreProvider _storeProvider;

        public SqlStreamReaderShould(EmbeddedMsSqlStreamStoreFixture fixture)
        {
            _fixture = fixture;
            _serializer = new SqlStreamStoreEventSerializer();
            _settingsProvider = new MsSqlStreamStoreSettingsProvider(_fixture.ConnectionString);
            _storeProvider = new MsSqlStreamStoreProvider(_settingsProvider);
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()
        {
            var aggregateId = Guid.NewGuid();
            var streamId =
                StreamIdBuilder.Create()
                .WithAggregateId(aggregateId)
                .WithAggregateType<DummyAggregate>()
                .Build();

            var reader = new SqlStreamStoreStreamReader(_storeProvider, _serializer);
            await Assert.ThrowsAsync<StreamNotFoundException>(async () =>
            {
                var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync()) ;
                await enumerator.DisposeAsync();
            });
        }
    }
}
