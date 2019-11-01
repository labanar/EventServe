using EventServe.EventStore.Interfaces;
using EventServe.Services;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public class EventStoreStreamSubscription<T> : BackgroundService
        where T : EventStreamSubscription, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventStoreStreamSubscription<T>> _logger;
        private bool _isResetRequested = false;
        private bool _stopped = false;

        public EventStoreStreamSubscription(
            IServiceProvider serviceProvider,
            ILogger<EventStoreStreamSubscription<T>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        //public async Task Reset()
        //{
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        var resetHandler = scope.ServiceProvider.GetRequiredService<IStreamSubscriptionResetHandler<T>>();
        //        if (resetHandler == null)
        //            return;

        //        _isResetRequested = true;
        //        var subscriptionInfo = new T();
        //        var stream = subscriptionInfo.Stream;

        //        var conn = GetConenctionProvider().GetConnection();
        //        await conn.ConnectAsync();

        //        var credentials = await GetConenctionProvider().GetCredentials();
        //        await conn.DeletePersistentSubscriptionAsync(stream.Id, subscriptionInfo.Name, credentials);


        //        await resetHandler.HandleStreamSubscriptionReset();

        //        //Reconnect to the subscription
        //        await ConnectToSubscription();

        //        _isResetRequested = false;
        //    }
        //}

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ConnectToSubscription();
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }
        }


        private IEventStoreConnectionProvider GetConenctionProvider()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var connectionProvider = scope.ServiceProvider.GetRequiredService<IEventStoreConnectionProvider>();
                return connectionProvider;
            }
        }

        private async Task HandleEvent(EventStorePersistentSubscriptionBase subscriptionBase, ResolvedEvent resolvedEvent)
        {
            _logger.LogInformation($"{new T().Name}: Event received: {resolvedEvent.Event.EventType} [{resolvedEvent.Event.EventId}]");

            var @event = DeserializeEvent(resolvedEvent);

            using (var scope = _serviceProvider.CreateScope())
            {
                var eventHandler = scope.ServiceProvider.GetRequiredService<IStreamSubscriptionEventHandler<T>>();
                await eventHandler.Handle(@event);
            }

            subscriptionBase.Acknowledge(resolvedEvent);

            _logger.LogInformation($"{new T().Name}: Event handled successfully: {resolvedEvent.Event.EventType} [{resolvedEvent.Event.EventId}]");
        }

        private Event DeserializeEvent(ResolvedEvent resolvedEvent)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventSerializer = scope.ServiceProvider.GetRequiredService<IEventSerializer>();
                return eventSerializer.DeseralizeEvent(resolvedEvent);
            }
        }

        private async  Task ConnectToSubscription()
        {
            var conn = GetConenctionProvider().GetConnection();
            await conn.ConnectAsync();

            var subscriptionInfo = new T();

            Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> processEvent = (a, b, c) => {
                return HandleEvent(a, b);
            };

            var stream = subscriptionInfo.Stream;   
            var credentials = await GetConenctionProvider().GetCredentials();
            _logger.LogInformation($"{new T().Name}: Creating subscription for stream {stream.Id}");
            await conn.CreateSubscription(stream.Id, subscriptionInfo.Name, credentials, _logger);
            await conn.ConnectToPersistentSubscriptionAsync(stream.Id, subscriptionInfo.Name, processEvent, bufferSize: 10, subscriptionDropped: SubscriptionDropped);      
            _logger.LogInformation($"{new T().Name}: Listening for events.");
        }


        private void SubscriptionDropped(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if (_isResetRequested)
                return;

            if (_stopped)
                return;

            _logger.LogError(ex, $"Subscription dropped: {subscriptionDropReason.ToString()}");
            var reconnectTask = ConnectToSubscription();
            reconnectTask.Wait();
        }
    }
}
