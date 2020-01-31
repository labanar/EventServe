using System;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public interface ISqlStreamStoreSubscriptionManager
    {
        Task CreateStreamSubscription(Guid subscriptionId, string streamId);
        Task PersistAcknowledgement(Guid subscriptionId, Guid evenTId);
        Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId);
    }

    public class SqlStreamStoreSubscriptionManager : ISqlStreamStoreSubscriptionManager
    {
        private readonly ISqlStreamStoreProvider _storeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly SqlStreamStoreStreamReader _reader;
        private readonly SqlStreamStoreStreamWriter _writer;
        private readonly EventRepository<SqlStreamStoreSubscriptionAggregate> _repository;

        public SqlStreamStoreSubscriptionManager(ISqlStreamStoreSubscriptionStoreProvider storeProvider, IEventSerializer eventSerializer)
        {
            _storeProvider = storeProvider;
            _eventSerializer = eventSerializer;
            _reader = new SqlStreamStoreStreamReader(_storeProvider, _eventSerializer);
            _writer = new SqlStreamStoreStreamWriter(_storeProvider, _eventSerializer);
            _repository = new EventRepository<SqlStreamStoreSubscriptionAggregate>(_writer, _reader);
        }

        public async Task CreateStreamSubscription(Guid subscriptionId, string streamId)
        {
            var subscription = new SqlStreamStoreSubscriptionAggregate(subscriptionId, streamId);
            await _repository.SaveAsync(subscription, subscription.Version);
        }

        public async Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId)
        {
            var subscription = await _repository.GetById(subscriptionId);
            if (subscription == null)
                return -1;
            return subscription.Position;
        }

        public async Task PersistAcknowledgement(Guid subscriptionId, Guid eventId)
        {
            var subscription = await _repository.GetById(subscriptionId);
            subscription.AcknowledgeEvent(eventId);
            await _repository.SaveAsync(subscription, subscription.Version);
        }
    }
}
