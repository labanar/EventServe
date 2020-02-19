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
            var enumerator =  GetAllEventsForAggregate(id).GetAsyncEnumerator();
            var aggregate = (T) Activator.CreateInstance(typeof(T), true);

            try
            {
                while (await enumerator.MoveNextAsync())
                    aggregate.LoadFromHistory(enumerator.Current);

                return aggregate;
            }
            catch { }
            finally
            {
                if(enumerator != null)
                    await enumerator.DisposeAsync();
            }

            return aggregate;
        }

        public async Task<long> SaveAsync(AggregateRoot aggregate, long version = -2) {
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

            if (version >= -1)
                await _streamWriter.AppendEventsToStream(streamId, events, version);
            else
                await _streamWriter.AppendEventsToStream(streamId, events);

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