using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EventServe.EventStore.Interfaces;
using EventServe.Subscriptions;
using EventStore.ClientAPI;
using MediatR;

namespace EventServe.EventStore.Subscriptions
{
    public class EventStorePersistentSubscription : PersistentStreamSubscription
    {
        private readonly ILogger<EventStorePersistentSubscription> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventStoreConnectionProvider _connectionProvider;

        private IEventStoreConnection _connection;
        private EventStorePersistentSubscriptionBase _subscriptionBase;
        private string _streamId;

        public EventStorePersistentSubscription(
            IEventSerializer eventSerializer,
            IEventStoreConnectionProvider connectionProvider,
            ILogger<EventStorePersistentSubscription> logger,
            IMediator mediator) : base(mediator)
        {
            _logger = logger;
            _eventSerializer = eventSerializer;
            _connectionProvider = connectionProvider;
        }

        public override async Task ConnectAsync(string streamId)
        {
            _streamId = streamId;
            await Connect();
        }

        private async Task Connect()
        {
            _connection = _connectionProvider.GetConnection();
            await _connection.ConnectAsync();
            await _connection.CreateSubscription(_streamId, _id.ToString(), await _connectionProvider.GetCredentials(), _logger);

            Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> processEvent = (subscriptionBase, resolvedEvent, c) => {
                return HandleEvent(subscriptionBase, resolvedEvent);
            };

            _subscriptionBase = await _connection.ConnectToPersistentSubscriptionAsync(
                _streamId,
                _id.ToString(),
                processEvent,
                bufferSize: 10,
                subscriptionDropped: SubscriptionDropped,
                autoAck: false);
        }

        private async Task HandleEvent(EventStorePersistentSubscriptionBase subscriptionBase, ResolvedEvent resolvedEvent)
        {
            var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
            await RaiseEvent(@event);
        }

        private void SubscriptionDropped(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            _connection.Dispose();
            Connect().Wait();   
        }

        protected override Task AcknowledgeEvent<T>(T @event)
        {
            if (_subscriptionBase == null)
                throw new ApplicationException("Subscription is not connected, therefore acknowledgement cannot be sent.");

            _subscriptionBase.Acknowledge(@event.EventId);
            return Task.CompletedTask;
        }
    }
}
