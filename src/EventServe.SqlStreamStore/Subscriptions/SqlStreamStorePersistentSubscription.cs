using EventServe.Subscriptions;
using MediatR;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStorePersistentSubscription : PersistentStreamSubscription
    {
        private readonly ILogger<SqlStreamStorePersistentSubscription> _logger;
        private readonly ISqlStreamStoreSubscriptionManager _subscriptionManager;
        private readonly IEventSerializer _eventSerializer;
        private readonly ISqlStreamStoreProvider _storeProvider;
        private IStreamStore _store;

        public SqlStreamStorePersistentSubscription(
            IEventSerializer eventSerializer,
            ISqlStreamStoreProvider storeProvider,
            ISqlStreamStoreSubscriptionManager subscriptionManager,
            ILogger<SqlStreamStorePersistentSubscription> logger,
            IMediator mediator) : base(mediator)
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
            _eventSerializer = eventSerializer;
            _storeProvider = storeProvider;
        }

        public override async Task ConnectAsync(string streamId)
        {
            //Get subscription position
            var pos = await _subscriptionManager.GetStreamSubscriptionPosition(_id);
            if (pos == null)
                await _subscriptionManager.CreateStreamSubscription(_id);


            _store = await _storeProvider.GetStreamStore();
            _store.SubscribeToAll(null,
                (_, message, cancellationToken) =>
                {
                    return HandleEvent(message, cancellationToken);
                },
                (sub, reason, ex) =>
                {
                    HandleSubscriptionDropped(sub, reason, ex);
                });
        }

        private async Task HandleEvent(StreamMessage message, CancellationToken cancellation)
        {
            _logger.LogInformation($"Event received: {message.Type} [{message.MessageId}]");
            var @event = await _eventSerializer.DeseralizeEvent(message);
            await RaiseEvent(@event);
            _logger.LogInformation($"Event rasied successfully: {message.Type} [{message.MessageId}]");
        }

        protected override async Task AcknowledgeEvent<T>(T @event)
        {
            await _subscriptionManager.PersistAcknowledgement(_id, @event);

            //We need to wait for the acknowledgement to 
        }

        private void HandleSubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception = null)
        {
            _logger.LogInformation($"{subscription.Name} subscription dropped: {reason.ToString()}");
            _connected = false;
        }
    }
}
