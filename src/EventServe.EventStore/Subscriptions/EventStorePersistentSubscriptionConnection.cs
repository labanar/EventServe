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

        protected override Task DisconnectAsync()
        {
            if (_subscriptionBase == null)
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

        protected override async Task ResetAsync()
        {
            var streamId = _streamId == null ? $"$ce-{_aggregateType.ToUpper()}" : _streamId.Id;
            using var connection = _connectionProvider.GetConnection();
            await connection.ConnectAsync();
            await connection.DeletePersistentSubscriptionAsync(streamId, _subscriptionName, await _connectionProvider.GetCredentials());
        }

        protected override Task AcknowledgeEvent(Guid eventId)
        {
            if (_subscriptionBase == null)
                throw new ApplicationException("Subscription is not connected, therefore acknowledgement cannot be sent.");

            _subscriptionBase.Acknowledge(eventId);
            return Task.CompletedTask;
        }

        private async Task Connect()
        {
            _connection = _connectionProvider.GetConnection();
            await _connection.ConnectAsync();


            var streamId = _streamId == null ? $"$ce-{_aggregateType.ToUpper()}" : _streamId.Id;

            //Check if this subscription already exists
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
            Func<Event> lazyEvent =
                new Func<Event>(() =>
                {
                    var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
                    @event.EventId = resolvedEvent.OriginalEvent.EventId;
                    return @event;
                });

            var streamMessage = new SubscriptionMessage(
                resolvedEvent.OriginalEvent.EventId,
                resolvedEvent.Event.EventStreamId,
                resolvedEvent.Event.EventType, 
                lazyEvent);

            _position = resolvedEvent.OriginalEventNumber;
            await RaiseMessage(streamMessage);
        }

        private void SubscriptionDropped(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if(_cancellationRequestedByUser)
            {
                if (_connection != null)
                    _connection.Dispose();

                _logger.LogInformation(ex, $"Subscription stopped by user: {subscriptionDropReason.ToString()}");
                return;
            }

            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            _connection.Dispose();
            Connect().Wait();   
        }
    }
}
