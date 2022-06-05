using EventServe.EventStore.IntegrationTests;
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
        private readonly SqlStreamStoreFixture _fixture;
        private readonly SqlStreamStoreEventSerializer _serializer;

        public SqlStreamReaderShould(SqlStreamStoreFixture fixture)
        {
            _fixture = fixture;
            _serializer = new SqlStreamStoreEventSerializer();
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()     
        {
            await using (var sandbox = await _fixture.CreateMsSqlSandbox())
            {
                var settingsProvider = new MsSqlStreamStoreSettingsProvider(sandbox.ConnectionString, "TestSchema");
                var storeProvider = new MsSqlStreamStoreProvider(settingsProvider);

                var aggregateId = Guid.NewGuid();
                var streamId =
                    StreamIdBuilder.Create()
                    .WithAggregateId(aggregateId)
                    .WithAggregateType<DummyAggregate>()
                    .Build();

                var reader = new SqlStreamStoreStreamReader(storeProvider, _serializer);
                await Assert.ThrowsAsync<StreamNotFoundException>(async () =>
                {
                    var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync()) ;
                    await enumerator.DisposeAsync();
                });
            }
        }
    }
}
