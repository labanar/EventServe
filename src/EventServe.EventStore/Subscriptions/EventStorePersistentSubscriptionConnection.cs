using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EventServe.EventStore.Interfaces;
using EventServe.Subscriptions;
using EventStore.ClientAPI;

namespace EventServe.EventStore.Subscriptions
{
    public class EventStorePersistentSubscriptionConnection : PersistentStreamSubscriptionConnection
    {
        private readonly ILogger<EventStorePersistentSubscriptionConnection> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventStoreConnectionProvider _connectionProvider;

        private IEventStoreConnection _connection;
        private EventStorePersistentSubscriptionBase _subscriptionBase;

        public EventStorePersistentSubscriptionConnection(
            IEventSerializer eventSerializer,
            IEventStoreConnectionProvider connectionProvider,
            ILogger<EventStorePersistentSubscriptionConnection> logger)
        {
            _logger = logger;
            _eventSerializer = eventSerializer;
            _connectionProvider = connectionProvider;
        }

        protected override async Task ConnectAsync()
        {
            _cancellationRequestedByUser = false;
            _connected = false;
            await Connect();
        }

        private async Task Connect()
        {
            _connection = _connectionProvider.GetConnection();
            await _connection.ConnectAsync();


            var streamId = _filter.SubscribedStreamId == null ? $"$ce-{_filter.AggregateType.Name.ToUpper()}" : _filter.SubscribedStreamId.Id;
            await _connection.CreateSubscription(streamId,
                                                 _subscriptionName,
                                                 await _connectionProvider.GetCredentials(),
                                                 _logger);


            Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> processEvent = (subscriptionBase, resolvedEvent, c) => {
                return HandleEvent(subscriptionBase, resolvedEvent);
            };

            _subscriptionBase = await _connection.ConnectToPersistentSubscriptionAsync(
                streamId,
                _subscriptionName,
                processEvent,
                bufferSize: 10,
                subscriptionDropped: SubscriptionDropped,
                autoAck: false);
            _connected = true;
        }

        private async Task HandleEvent(EventStorePersistentSubscriptionBase subscriptionBase, ResolvedEvent resolvedEvent)
        {
            if (_filter != null && !_filter.DoesEventPassFilter(resolvedEvent.Event.EventType, resolvedEvent.Event.EventStreamId))
                await AcknowledgeEvent(resolvedEvent.OriginalEvent.EventId);

            var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
            @event.EventId = resolvedEvent.OriginalEvent.EventId;
            await RaiseEvent(@event);
        }

        private void SubscriptionDropped(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if(_cancellationRequestedByUser)
            {
                _logger.LogInformation(ex, $"Subscription stopped by user: {subscriptionDropReason.ToString()}");
                return;
            }

            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            _connection.Dispose();
            Connect().Wait();   
        }

        protected override Task AcknowledgeEvent(Guid eventId)
        {
            if (_subscriptionBase == null)
                throw new ApplicationException("Subscription is not connected, therefore acknowledgement cannot be sent.");

            _subscriptionBase.Acknowledge(eventId);
            return Task.CompletedTask;
        }

        protected override Task DisconnectAsync()
        {
            if(_subscriptionBase == null)
            {
                _connected = false;
                _cancellationRequestedByUser = true;
                return Task.CompletedTask;
            }

            _cancellationRequestedByUser = true;
            _connection.Close();
            _connection.Dispose();
            return Task.CompletedTask;
        }
    }
}
