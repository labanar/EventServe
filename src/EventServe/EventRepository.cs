using EventServe.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe
{
    public class EventRepository<T> : IEventRepository<T>
        where T: AggregateRoot
    {
        private readonly IEventStreamWriter _streamWriter;
        private readonly IEventStreamReader _streamReader;
        private readonly ILogger<EventRepository<T>> _logger;

        public EventRepository(
            IEventStreamWriter streamWriter,
            IEventStreamReader streamReader,
            ILogger<EventRepository<T>> logger)
        {
            _streamWriter = streamWriter;
            _streamReader = streamReader;
            _logger = logger;
        }

        public async Task<T> GetById(Guid id)
        {
            var events = await GetAllEventsForAggregate(id);

            if (events.Count == 0)
                return null;

            var aggregate = (T)Activator.CreateInstance(typeof(T), true);
            aggregate.LoadFromHistory(events);
            return aggregate;       
        }

        public async Task<long> SaveAsync(AggregateRoot aggregate, long version = 0)
        {
            if (aggregate == null)
                return 0;

            //Save the uncomitted changes to the eventstore
            var events = aggregate.GetUncommitedChanges();
            if (!events.Any())
                return 0;

            var stream =
                StreamBuilder.Create()
                .FromAggregateRoot(aggregate)
                .Build();

            var eventCount = events.Count();

            if (version != 0)
                await _streamWriter.AppendEventsToStream(stream.Id, events, version);
            else
                await _streamWriter.AppendEventsToStream(stream.Id, events);

            aggregate.MarkChangesAsCommitted();
            return eventCount;
        }

        private async Task<List<Event>> GetAllEventsForAggregate(Guid id)
        {
            try
            {
                var stream = StreamBuilder.Create()
                    .WithAggregateType<T>()
                    .WithAggregateId(id)
                    .Build();

                return await _streamReader.ReadAllEventsFromStream(stream.Id);
            }
            catch (StreamDeletedException streamDeleted)
            {
                _logger.LogWarning(streamDeleted, streamDeleted.Message);
                throw;
            }
            //Stream not found usually indicates that the stream has not yet been created, log and swallow
            catch (StreamNotFoundException streamNotFound)
            {
                _logger.LogWarning(streamNotFound, streamNotFound.Message);
                return new List<Event>();
            }
        }
    }
}
