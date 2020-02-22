using EventServe.Subscriptions;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IStreamSubscription = SqlStreamStore.IStreamSubscription;
using TransientStreamSubscriptionConnection = EventServe.Subscriptions.TransientStreamSubscriptionConnection;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreTransientSubscriptionConnection : TransientStreamSubscriptionConnection
    {
        private readonly ILogger<SqlStreamStoreTransientSubscriptionConnection> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly ISqlStreamStoreProvider _storeProvider;
        private IStreamStore _store;
        private IAllStreamSubscription _allSubscription;
        private IStreamSubscription _streamSubscription;

        public SqlStreamStoreTransientSubscriptionConnection(
            IEventSerializer eventSerializer,
            ISqlStreamStoreProvider storeProvider,
            ILogger<SqlStreamStoreTransientSubscriptionConnection> logger)
        {
            _logger = logger;
            _eventSerializer = eventSerializer;
            _storeProvider = storeProvider;
        }

        protected override async Task ConnectAsync()
        {
            _store = await _storeProvider.GetStreamStore();

            if (_streamId == null)
            {
                _allSubscription = _store.SubscribeToAll(
                    _startPosition == StreamPosition.End ? -1 : 0,
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
                _streamSubscription = _store.SubscribeToStream(
                    _streamId.Id, 
                    _startPosition == StreamPosition.End ? -1 : 0,
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


        private void HandleSubscriptionDropped(IStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception = null)
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
            if (_allSubscription != null)
            {
                _cancellationRequestedByUser = true;
                _allSubscription.Dispose();
            }
            else if (_streamSubscription != null)
            {
                _cancellationRequestedByUser = true;
                _streamSubscription.Dispose();
            }

            return Task.CompletedTask;
        }
    }

}
