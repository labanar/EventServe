using System;
using System.Threading.Tasks;
using EventServe.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{

    [Collection("EventStore Collection")]
    public class EventStoreRepositoryShould
    {
        private readonly EventSerializer _serializer;
        private readonly EmbeddedEventStoreFixture _fixture;

        public EventStoreRepositoryShould(EmbeddedEventStoreFixture fixture)
        {
            _serializer = new EventSerializer();
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_aggregate_returns_null_when_aggregate_does_not_exist()
        {
            var sut = CreateRepository<DummyAggregate>();
            var result = await sut.GetById(Guid.NewGuid());
            result.Should().Be(null);    
        }

        [Fact]
        public async Task Writes_aggregate_to_stream()
        {

            var fakeAggregate = await CreateDummyAggregate();

            var reader = CreateStreamReader();
            var streamId = StreamIdBuilder.Create()
                .WithAggregateType<DummyAggregate>()
                .WithAggregateId(fakeAggregate.Id)
                .Build();

            fakeAggregate.Version.Should().Be(0);


            var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
            var count = 0;
            while (await enumerator.MoveNextAsync())
                count++;
            await enumerator.DisposeAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task Read_and_writes_change_to_aggregate()
        {
            var fakeAggregate = await CreateDummyAggregate();
            var sut = CreateRepository<DummyAggregate>();
            var aggregate = await sut.GetById(fakeAggregate.Id);

            aggregate.ResetDummy(new ResetDummyAggregateCommand
            {
                Id = aggregate.Id,
                Name = "A new name",
                Url = "https://url.example.com"
            });

            await sut.SaveAsync(aggregate);
        }

        [Fact]
        public async Task Write_fails_when_aggregate_has_been_modified_by_another_actor()
        {
            //Create a fake aggregate
            var fakeAggregate = await CreateDummyAggregate();

            var sut = CreateRepository<DummyAggregate>();

            //Query the fake aggregate two different times, emulating two actors
            var aggregate1 = await sut.GetById(fakeAggregate.Id);
            var aggregate2 = await sut.GetById(fakeAggregate.Id);

            //Modify each aggregate
            aggregate1.ResetDummy(new ResetDummyAggregateCommand
            {
                Id = fakeAggregate.Id,
                Name = "A new name",
                Url = "https://url.example.com"
            });

            aggregate2.ResetDummy(new ResetDummyAggregateCommand
            {
                Id = fakeAggregate.Id,
                Name = "The name",
                Url = "https://newurl.example.com"
            });

            //Save the first aggregate
            await sut.SaveAsync(aggregate1);

            //Saving the second aggregate should fail, as it has been modified since we queried
            await Assert.ThrowsAsync<WrongExpectedVersionException>(async () => await sut.SaveAsync(aggregate2));
        }


        private async Task<DummyAggregate> CreateDummyAggregate()
        {
            var resetCommand = new ResetDummyAggregateCommand()
            {
                Id = Guid.NewGuid(),
                Name = "The name",
                Url = "https://url.example.com"
            };

            var aggregate = new DummyAggregate(resetCommand);

            var sut = CreateRepository<DummyAggregate>();
            await sut.SaveAsync(aggregate);

            return aggregate;
        }

        private EventRepository<T> CreateRepository<T>()
        where T : AggregateRoot
        {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));

            var reader = new EventStoreStreamReader(connectionProvider, _serializer);
            var writer = new EventStoreStreamWriter(connectionProvider, _serializer);

            var sut = new EventRepository<T>(writer, reader);

            return sut;
        }

        private IEventStreamReader CreateStreamReader()
        {
            var connectionProvider = new EventStoreConnectionProvider(Options.Create(_fixture.EventStoreConnectionOptions));
            return new EventStoreStreamReader(connectionProvider, _serializer);
        }

    }
}