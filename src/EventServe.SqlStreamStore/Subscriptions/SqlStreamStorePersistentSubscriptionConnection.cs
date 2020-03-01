using EventServe.Subscriptions;
using EventServe.Subscriptions.Enums;
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
            _position = await _subscriptionManager.GetSubscriptionPosition(_subscriptionId, true);
            _store = await _storeProvider.GetStreamStore();

            if(_streamId == null)
            {
                _allSubscription = _store.SubscribeToAll(_position,
                   HandleSubscriptionEvent,
                   HandleSubscriptionDropped);
                _status = SubscriptionConnectionStatus.Connected;
                _startDate = DateTime.UtcNow;
            }
            else
            {
                int? intPos = (_position != null) ? Convert.ToInt32(_position) : default(int?);
                _subscription = _store.SubscribeToStream(_streamId.Id, intPos,
                   HandleSubscriptionEvent,
                   HandleSubscriptionDropped);
                _status = SubscriptionConnectionStatus.Connected;
                _startDate = DateTime.UtcNow;
            }
        }

        protected override Task DisconnectAsync()
        {
            if (_subscription == null && _allSubscription == null)
                return Task.CompletedTask;

            _cancellationRequestedByUser = true;

            if(_subscription != null)
                _subscription.Dispose();

            if (_allSubscription != null)
                _allSubscription.Dispose();

            if (_store != null)
                _store.Dispose();

            _status = SubscriptionConnectionStatus.Disconnected;
            return Task.CompletedTask;
        }
        protected override async Task ResetAsync()
        {
            await _subscriptionManager.ResetSubscriptionPosition(_subscriptionId);
        }

        private async Task HandleEvent(StreamMessage message, CancellationToken cancellation)
        {
            Func<Event> lazyEvent =
                new Func<Event>(() => 
                {
                    var deserializationTask = _eventSerializer.DeseralizeEvent(message);
                    deserializationTask.Wait();
                    return deserializationTask.Result;
                });

            var streamMessage = new SubscriptionMessage(message.MessageId, message.StreamId, message.Type, lazyEvent);
            await RaiseMessage(streamMessage);
        }
        protected override async Task AcknowledgeEvent(Guid eventId)
        {
            _position = (_position == null) ? 0 : ++_position;
            await _subscriptionManager.SetSubscriptionPosition(_subscriptionId, _position);
        }

        private Task HandleSubscriptionEvent(IAllStreamSubscription subscription, StreamMessage message, CancellationToken token)
        {
            return HandleEvent(message, token);
        }
        private Task HandleSubscriptionEvent(IStreamSubscription subscription, StreamMessage message, CancellationToken token)
        {
            return HandleEvent(message, token);
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
    }
}
