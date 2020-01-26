using EventServe.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore
{
    public class SqlStreamStoreSubscription<T>: BackgroundService
        where T: EventStreamSubscription, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SqlStreamStoreSubscription<T>> _logger;

        public SqlStreamStoreSubscription(
            IServiceProvider serviceProvider,
            ILogger<SqlStreamStoreSubscription<T>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectToSubscription();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }
        }

        private async Task ConnectToSubscription()
        {
            Func<IAllStreamSubscription, StreamMessage, CancellationToken, Task> processEvent = (a, b, c) => {
                return HandleEvent(a, b, c);
            };

            var store = await GetStreamStore();
            store.SubscribeToAll(null,
                (subscription, message, cancellationToken) =>
                {
                    return HandleEvent(subscription, message, cancellationToken);
                },
                (sub, reason, ex) =>
                {
                    HandleSubscriptionDropped(sub, reason, ex);
                });
        }

        private async Task<IStreamStore> GetStreamStore()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<ISqlStreamStoreProvider>();
                return await service.GetStreamStore();
            }
        }

        private async Task<Event> DeserializeEvent(StreamMessage streamMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventSerializer = scope.ServiceProvider.GetRequiredService<IEventSerializer>();
                return await eventSerializer.DeseralizeEvent(streamMessage);
            }
        }

        private async Task HandleEvent(IAllStreamSubscription subscription, StreamMessage message, CancellationToken cancellation)
        {
            _logger.LogInformation($"{new T().Name}: Event received: {message.Type} [{message.MessageId}]");

            var @event = await DeserializeEvent(message);

            using (var scope = _serviceProvider.CreateScope())
            {
                var eventHandler = scope.ServiceProvider.GetRequiredService<IStreamSubscriptionEventHandler<T>>();
                await eventHandler.Handle(@event);
            }

            _logger.LogInformation($"{new T().Name}: Event handled successfully: {message.Type} [{message.MessageId}]");
        }

        private void HandleSubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception = null)
        {
            _logger.LogInformation($"{subscription.Name} subscription dropped: {reason.ToString()}");
        }


    }
}
