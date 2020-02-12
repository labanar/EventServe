using EventServe.Subscriptions;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IStreamSubscription = SqlStreamStore.IStreamSubscription;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStorePersistentSubscriptionConnection : PersistentStreamSubscriptionConnection
    {
        private readonly ILogger<SqlStreamStorePersistentSubscriptionConnection> _logger;
        private readonly IPersistentSubscriptionPositionManager _subscriptionManager;
        private readonly IEventSerializer _eventSerializer;
        private readonly ISqlStreamStoreProvider _storeProvider;
        private IStreamStore _store;
        private IStreamSubscription _subscription;
        private IAllStreamSubscription _allSubscription;

        public SqlStreamStorePersistentSubscriptionConnection(
            IEventSerializer eventSerializer,
            ISqlStreamStoreProvider storeProvider,
            IPersistentSubscriptionPositionManager subscriptionManager,
            ILogger<SqlStreamStorePersistentSubscriptionConnection> logger) : base()
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
            _eventSerializer = eventSerializer;
            _storeProvider = storeProvider;
        }

        protected override async Task ConnectAsync()
        {
            //Get subscription position
            var pos = await _subscriptionManager.GetSubscriptionPosition(_subscriptionName);
            int? intPos = (pos != null) ? Convert.ToInt32(pos) : default(int?);

            _store = await _storeProvider.GetStreamStore();

            if(_filter.SubscribedStreamId == null)
            {
                _allSubscription = _store.SubscribeToAll(intPos,
                   (_, message, cancellationToken) =>
                   {
                       return HandleEvent(message, cancellationToken);
                   },
                   (sub, reason, ex) =>
                   {
                       HandleSubscriptionDropped(sub, reason, ex);
                   });
            }
            else
            {
                _subscription = _store.SubscribeToStream(_filter.SubscribedStreamId.Id, intPos,
                   (_, message, cancellationToken) =>
                   {
                       return HandleEvent(message, cancellationToken);
                   },
                   (sub, reason, ex) =>
                   {
                       HandleSubscriptionDropped(sub, reason, ex);
                   });
            }


           
        }

        private async Task HandleEvent(StreamMessage message, CancellationToken cancellation)
        {
            //Check if this event passes through the filter
            if (_filter != null && !_filter.DoesEventPassFilter(message.Type, message.StreamId))
                await AcknowledgeEvent(message.MessageId);

            _logger.LogInformation($"Event received: {message.Type} [{message.MessageId}]");
            var @event = await _eventSerializer.DeseralizeEvent(message);
            await RaiseEvent(@event);
            _logger.LogInformation($"Event rasied successfully: {message.Type} [{message.MessageId}]");
        }

        protected override async Task AcknowledgeEvent(Guid eventId)
        {
            await _subscriptionManager.IncrementSubscriptionPosition(_subscriptionName);
        }

        private void HandleSubscriptionDropped(IStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception = null)
        {
            if(_cancellationRequestedByUser)
            {
                _logger.LogInformation( $"Subscription stopped by user: {subscription.Name}");
                _connected = false;
                return;
            }

            if (exception != null)
                _logger.LogError(exception, $"{subscription.Name} subscription dropped: {reason.ToString()}");
            else
                _logger.LogError($"{subscription.Name} subscription dropped: {reason.ToString()}");
            _connected = false;
        }

        private void HandleSubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception = null)
        {
            if (_cancellationRequestedByUser)
            {
                _logger.LogInformation($"Subscription stopped by user: {subscription.Name}");
                _connected = false;
                return;
            }

            if (exception != null)
                _logger.LogError(exception, $"{subscription.Name} subscription dropped: {reason.ToString()}");
            else
                _logger.LogError($"{subscription.Name} subscription dropped: {reason.ToString()}");
            _connected = false;
        }

        protected override Task DisconnectAsync()
        {
            if (_subscription == null)
                return Task.CompletedTask;

            _cancellationRequestedByUser = true;
            _subscription.Dispose();
            return Task.CompletedTask;
        }
    }
}
