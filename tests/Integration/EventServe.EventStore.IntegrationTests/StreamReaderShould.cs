using Bogus;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    [Collection("EventStore Collection")]
    public class StreamReaderShould
    {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public StreamReaderShould(EmbeddedEventStoreFixture fixture)
        {
            _serializer = new EventSerializer();
            _fixture = fixture;
        }

        [Fact]
        public async Task Throw_stream_not_found_exception_if_stream_does_not_exist()
        {
            await using (var sandbox = await _fixture.CreateEventStoreSandbox())
            {
                var aggregateId = Guid.NewGuid();
                var streamId =
                    StreamIdBuilder.Create()
                    .WithAggregateId(aggregateId)
                    .WithAggregateType<DummyAggregate>()
                    .Build();

                var reader = new EventStoreStreamReader(sandbox.ConnectionProvider, _serializer);
                await Assert.ThrowsAsync<StreamNotFoundException>(async () =>
                {
                    var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync()) ;
                    await enumerator.DisposeAsync();
                });
            }
        }

        [Fact]
        public async Task Return_all_events_in_order_when_reading_from_beginning()
        {
            await using (var sandbox = await _fixture.CreateEventStoreSandbox())
            {
                var writer = new EventStoreStreamWriter(sandbox.ConnectionProvider, _serializer);
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                long expectedSequenceNumber = Faker.GlobalUniqueIndex - 4000;

                var sut = new EventStoreStreamReader(sandbox.ConnectionProvider, _serializer);
                var eventsSeen = 0;
                await foreach (TestEvent @event in sut.ReadAllEventsFromStreamAsync("test-stream"))
                {
                    @event.SequenceNumber.Should().Be(++expectedSequenceNumber);
                    eventsSeen++;
                }

                eventsSeen.Should().Be(4_000);
            }
        }

        [Fact]
        public async Task Return_all_events_in_reverse_order_when_reading_backwards()
        {
            await using (var sandbox = await _fixture.CreateEventStoreSandbox())
            {
                var writer = new EventStoreStreamWriter(sandbox.ConnectionProvider, _serializer);
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));
                await writer.AppendEventsToStream("test-stream", SequentialEventGenerator(1_000));

                var sut = new EventStoreStreamReader(sandbox.ConnectionProvider, _serializer);
                var eventsSeen = 0;
                long expectedSequenceNumber = Faker.GlobalUniqueIndex;
                await foreach (TestEvent @event in sut.ReadStreamBackwards("test-stream"))
                {
                    @event.SequenceNumber.Should().Be(expectedSequenceNumber--);
                    eventsSeen++;
                }

                eventsSeen.Should().Be(4_000);
            }
        }

        public List<TestEvent> SequentialEventGenerator(int numToGenerate)
        {
            var faker = new Faker<TestEvent>()
                .RuleFor(x => x.SequenceNumber, (f, r) => f.IndexGlobal)
                .RuleFor(x => x.AggregateId, (f, r) => f.Database.Random.Guid())
                .RuleFor(x => x.EventId, (f, r) => f.Database.Random.Guid());

            return faker.Generate(numToGenerate);
        }
    }

    public class TestEvent : Event
    {
        public TestEvent() { }

        public TestEvent(Guid aggregateId, long sequenceNumber) : base(aggregateId)
        {
            SequenceNumber = sequenceNumber;
        }

        public long SequenceNumber { get; set; }
    }
}