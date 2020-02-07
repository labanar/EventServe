using EventServe.EventStore.Interfaces;
using EventServe.Subscriptions;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using ESSubscription = EventStore.ClientAPI.EventStoreSubscription;

namespace EventServe.EventStore.Subscriptions
{
    public class EventStoreTransientSubscription : TransientStreamSubscription
    {
        private readonly ILogger<EventStorePersistentSubscription> _logger;
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventStoreConnectionProvider _connectionProvider;

        private IEventStoreConnection _connection;
        private string _streamId;
        private ESSubscription _subscriptionBase;

        public EventStoreTransientSubscription(
            IEventSerializer eventSerializer,
            IEventStoreConnectionProvider connectionProvider,
            ILogger<EventStorePersistentSubscription> logger)
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
            await _connection.CreateSubscription(_streamId, Guid.NewGuid().ToString(), await _connectionProvider.GetCredentials(), _logger);

            Func<ESSubscription, ResolvedEvent, Task> processEvent = (subscriptionBase, resolvedEvent) => {
                return HandleEvent(subscriptionBase, resolvedEvent);
            };

            if(_filter.SubscribedStreamId == StreamId.All)
            {
                _subscriptionBase = await _connection.SubscribeToAllAsync(
                true,
                processEvent,
                subscriptionDropped: SubscriptionDropped);
                _connected = true;
            }
            else
            {
                _subscriptionBase = await _connection.SubscribeToStreamAsync(
                    _filter.SubscribedStreamId.Id,
                    true,
                    processEvent,
                    subscriptionDropped: SubscriptionDropped);
                                _connected = true;
            }
        }

        private async Task HandleEvent(ESSubscription subscriptionBase, ResolvedEvent resolvedEvent)
        {
            var @event = _eventSerializer.DeseralizeEvent(resolvedEvent);
            await RaiseEvent(@event, resolvedEvent.OriginalStreamId);
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
