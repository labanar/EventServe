using EventServe.EventStore.Interfaces;
using EventServe.Subscriptions;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using ESSubscription = EventStore.ClientAPI.EventStoreSubscription;

namespace EventServe.EventStore.Subscriptions
{
    public class EventStoreTransientSubscriptionConnection : TransientStreamSubscriptionConnection
    {
        private readonly ILogger<EventStorePersistentSubscriptionConnection> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventStoreConnectionProvider _connectionProvider;

        private IEventStoreConnection _connection;
        private ESSubscription _subscriptionBase;
        private EventStoreCatchUpSubscription _catchUpSubscriptionBase;

        public EventStoreTransientSubscriptionConnection(
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


            Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> processEvent = (subscriptionBase, resolvedEvent) => {
                return HandleEvent(resolvedEvent);
            };

            Func<ESSubscription, ResolvedEvent, Task> processEventAlt = (subscriptionBase, resolvedEvent) => {
                return HandleEvent(resolvedEvent);
            };

            var streamId = _filter.SubscribedStreamId == null ? $"$ce-{_filter.AggregateType.Name.ToUpper()}" : _filter.SubscribedStreamId.Id;


            if (_startPosition == -1)
            {
                _connected = true;
                _subscriptionBase = await _connection.SubscribeToStreamAsync(
                    streamId,
                    true,
                    processEventAlt,
                    subscriptionDropped: SubscriptionDropped);
            }
            else
            {
                _connected = true;
                _catchUpSubscriptionBase = _connection.SubscribeToStreamFrom(
                    streamId,
                    _startPosition,
                    new CatchUpSubscriptionSettings(
                        CatchUpSubscriptionSettings.Default.MaxLiveQueueSize,
                        CatchUpSubscriptionSettings.Default.ReadBatchSize,
                        false,
                        true,
                        CatchUpSubscriptionSettings.Default.SubscriptionName),
                    processEvent,
                    subscriptionDropped: SubscriptionDropped);

            }
        }

        private async Task HandleEvent(ResolvedEvent resolvedEvent)
        {
            //Check if this event passes through the filter
            if (_filter != null && !_filter.DoesEventPassFilter(resolvedEvent.Event.EventType, resolvedEvent.Event.EventStreamId))
                return;

            var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
            @event.EventId = resolvedEvent.OriginalEvent.EventId;
            await RaiseEvent(@event);
        }


        private void SubscriptionDropped(EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if (_cancellationRequestedByUser)
            {
                _logger.LogInformation(ex, $"Subscription stopped by user: {subscriptionDropReason.ToString()}");
                return;
            }

            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            _connection.Dispose();
            Connect().Wait();
        }


        private void SubscriptionDropped(ESSubscription subscription,
           SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if (_cancellationRequestedByUser)
            {
                _logger.LogInformation(ex, $"Subscription stopped by user: {subscriptionDropReason.ToString()}");
                return;
            }

            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            _connection.Dispose();
            Connect().Wait();
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
    }
}
