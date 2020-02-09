using EventServe.Services;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.SqlStreamStore.IntegrationTests
{
    public class EventStoreRepositoryShould
    {
        private readonly IEventSerializer _serializer;
        private readonly InMemorySqlStreamStoreProvider _storeProvider;

        public EventStoreRepositoryShould()
        {
            _serializer = new SqlStreamStoreEventSerializer();
            _storeProvider = new InMemorySqlStreamStoreProvider();
        }

        [Fact]
        public async Task Writes_aggregate_to_stream()
        {
            var sut = await CreateRepository<DummyAggregate>();

            var fakeAggregate = await CreateDummyAggregate(sut);

            var reader = CreateStreamReader();
            var streamId = StreamIdBuilder.Create()
                .WithAggregateType<DummyAggregate>()
                .WithAggregateId(fakeAggregate.Id)
                .Build();

            fakeAggregate.Version.Should().Be(0);

            var count = 0;
            var enumerator = reader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
                count++;
            await enumerator.DisposeAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task Write_fails_when_aggregate_has_been_modified_by_another_actor()
        {
            var sut = await CreateRepository<DummyAggregate>();

            //Create a fake aggregate
            var fakeAggregate = await CreateDummyAggregate(sut);


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
            await sut.SaveAsync(aggregate1, aggregate1.Version);

            //Saving the second aggregate should fail, as it has been modified since we queried
            await Assert.ThrowsAsync<WrongExpectedVersionException>(async () => await sut.SaveAsync(aggregate2, aggregate2.Version));
        }


        private async Task<DummyAggregate> CreateDummyAggregate(IEventRepository<DummyAggregate> repository)
        {
            var resetCommand = new ResetDummyAggregateCommand()
            {
                Id = Guid.NewGuid(),
                Name = "The name",
                Url = "https://url.example.com"
            };

            var aggregate = new DummyAggregate(resetCommand);
            await repository.SaveAsync(aggregate, aggregate.Version);

            return aggregate;
        }

        private async Task<EventRepository<T>> CreateRepository<T>()
        where T : AggregateRoot
        {

            var reader = new SqlStreamStoreStreamReader(_storeProvider, _serializer);
            var writer = new SqlStreamStoreStreamWriter(_storeProvider, _serializer);
            var sut = new EventRepository<T>(writer, reader);

            return sut;
        }

        private IEventStreamReader CreateStreamReader()
        {
            return new SqlStreamStoreStreamReader(_storeProvider, _serializer);
        }

    }
}
