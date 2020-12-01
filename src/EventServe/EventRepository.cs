using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventServe.Services;

namespace EventServe 
{
    public class EventRepository<T> : IEventRepository<T>
        where T : AggregateRoot 
    {
        private readonly IEventStreamWriter _streamWriter;
        private readonly IEventStreamReader _streamReader;

        public EventRepository(
            IEventStreamWriter streamWriter,
            IEventStreamReader streamReader) {
            _streamWriter = streamWriter;
            _streamReader = streamReader;
        }

        public async Task<T> GetById(Guid id) 
        {
            try
            {
                var seen = 0;
                var aggregate = (T)Activator.CreateInstance(typeof(T), true);

                await foreach(var @event in GetAllEventsForAggregate(id))
                {
                    seen += 1;
                    aggregate.LoadFromHistory(@event);
                }

                //No events? return null
                if (seen == 0)
                    return null;

                return aggregate;
            }
            catch(StreamNotFoundException snf)
            {
                return null;
            }
        }

        public async Task<long> SaveAsync(AggregateRoot aggregate) {
            if (aggregate == null)
                return 0;

            //Save the uncomitted changes to the eventstore
            var events = aggregate.GetUncommitedChanges();
            if (!events.Any())
                return 0;

            var streamId =
                StreamIdBuilder.Create()
                .FromAggregateRoot(aggregate)
                .Build();

            var eventCount = events.Count();
            await _streamWriter.AppendEventsToStream(streamId, events, aggregate.Version);

            aggregate.MarkChangesAsCommitted();
            return eventCount;
        }

        private async IAsyncEnumerable<Event> GetAllEventsForAggregate(Guid id) 
        {
            var streamId = StreamIdBuilder.Create()
                .WithAggregateType<T>()
                .WithAggregateId(id)
                .Build();

            var enumerator = _streamReader.ReadAllEventsFromStreamAsync(streamId).GetAsyncEnumerator();
            try
            {
                if (enumerator == null)
                    yield break;

                while (await enumerator.MoveNextAsync())
                    yield return enumerator.Current;
            }
            finally
            {
                if (enumerator != null)
                    await enumerator.DisposeAsync();
            }

            yield break;
        }
    }
}