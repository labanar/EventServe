using Microsoft.Extensions.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IStreamSubscription = SqlStreamStore.IStreamSubscription;
using TransientStreamSubscription = EventServe.Subscriptions.TransientStreamSubscription;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreTransientSubscription : TransientStreamSubscription
    {
        private readonly ILogger<SqlStreamStoreTransientSubscription> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly ISqlStreamStoreProvider _storeProvider;
        private  int _count;
        private IStreamStore _store;
        private IAllStreamSubscription _allSubscription;
        private IStreamSubscription _streamSubscription;

        public SqlStreamStoreTransientSubscription(
            IEventSerializer eventSerializer,
            ISqlStreamStoreProvider storeProvider,
            ILogger<SqlStreamStoreTransientSubscription> logger)
        {
            _logger = logger;
            _eventSerializer = eventSerializer;
            _storeProvider = storeProvider;
            _count = 0;
        }

        protected override async Task ConnectAsync()
        {
            _store = await _storeProvider.GetStreamStore();

            if (_filter.SubscribedStreamId == null)
            {
                _allSubscription = _store.SubscribeToAll(_startPosition,
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
                _streamSubscription = _store.SubscribeToStream(_filter.SubscribedStreamId.Id, _startPosition,
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
            //_logger.LogInformation($"Event received: {message.Type} [{message.MessageId}]");
            _logger.LogInformation($"[{_count.ToString().PadLeft(6, '0')}] {message.Type} [{message.MessageId}]");
            var @event = await _eventSerializer.DeseralizeEvent(message);
            await RaiseEvent(@event, message.StreamId);
            _count+=1;
            //_logger.LogInformation($"Event rasied successfully: {message.Type} [{message.MessageId}]");
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
