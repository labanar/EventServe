using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamId = SqlStreamStore.Streams.StreamId;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public interface ISqlStreamStoreSubscriptionManager
    {
        Task CreateStreamSubscription(Guid subscriptionId, string streamId);
        Task<IEnumerable<SqlStreamStoreSubscriptionPosition>> GetAllStreamSubscriptions();
        Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId);
        Task PersistAcknowledgement(Guid subscriptionId, Guid eventId);
    }

    public class SqlStreamStoreSubscriptionManager : ISqlStreamStoreSubscriptionManager
    {
        private readonly ISqlStreamStoreProvider _storeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly SqlStreamStoreStreamReader _reader;
        private readonly SqlStreamStoreStreamWriter _writer;
        private readonly EventRepository<SqlStreamSubscriptionManagerAggregate> _repository;

        public SqlStreamStoreSubscriptionManager(ISqlStreamStoreSubscriptionStoreProvider storeProvider, IEventSerializer eventSerializer)
        {
            _storeProvider = storeProvider;
            _eventSerializer = eventSerializer;
            _reader = new SqlStreamStoreStreamReader(_storeProvider, _eventSerializer);
            _writer = new SqlStreamStoreStreamWriter(_storeProvider, _eventSerializer);
            _repository = new EventRepository<SqlStreamSubscriptionManagerAggregate>(_writer, _reader);
        }

        public async Task CreateStreamSubscription(Guid subscriptionId, string streamId)
        {
            var manager = await _repository.GetById(Guid.Empty);
            if (manager == null) manager = new SqlStreamSubscriptionManagerAggregate();
            manager.CreateStreamSubscription(subscriptionId, streamId);
            await _repository.SaveAsync(manager, manager.Version);
        }

        public async Task<IEnumerable<SqlStreamStoreSubscriptionPosition>> GetAllStreamSubscriptions()
        {
            var manager = await _repository.GetById(Guid.Empty);
            if (manager == null) manager = new SqlStreamSubscriptionManagerAggregate();
            return manager.Subscriptions;
        }

        public async Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId)
        {
            //We could load this through the repository, however
            //replaying the events to hydrate the aggregate is not
            var streamId =
                StreamIdBuilder.Create()
                    .FromStreamId($"SQLSTREAMSTORESUBSCRIPTION-{subscriptionId}")
                    .Build();

            var store = await _storeProvider.GetStreamStore();
            var eventSlice = await store.ReadStreamBackwards(new StreamId(streamId), StreamVersion.End, 1);
            if (eventSlice.Messages.Length == 0)
                return null;

            return eventSlice.LastStreamVersion;
        }

        public async Task PersistAcknowledgement(Guid subscriptionId, Guid eventId)
        {
            var @event = new SqlStreamStoreSubscriptionAcknowledgeEvent(subscriptionId, eventId);
            var streamId =
                StreamIdBuilder.Create()
                    .FromStreamId($"SQLSTREAMSTORESUBSCRIPTION-{subscriptionId}")
                    .Build();

            await _writer.AppendEventToStream(streamId, @event);
        }
    }
}
