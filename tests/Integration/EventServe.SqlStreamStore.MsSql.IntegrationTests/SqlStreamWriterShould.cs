using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    [Collection("SqlStreamStore Collection")]
    public class SqlStreamWriterShould
    {
        private readonly EmbeddedMsSqlStreamStoreFixture _fixture;
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly MsSqlStreamStoreSettingsProvider _settingsProvider;
        private readonly MsSqlStreamStoreProvider _storeProvider;

        public SqlStreamWriterShould(EmbeddedMsSqlStreamStoreFixture fixture)
        {
            _fixture = fixture;
            _serializer = new SqlStreamStoreEventSerializer();
            _settingsProvider = new MsSqlStreamStoreSettingsProvider(_fixture.ConnectionString);
            _storeProvider = new MsSqlStreamStoreProvider(_settingsProvider);
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
