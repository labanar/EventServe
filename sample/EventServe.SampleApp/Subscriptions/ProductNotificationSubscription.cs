using EventServe.SampleApp.Domain;
using EventServe.SampleApp.Domain.Events;
using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Subscriptions
{
    public class ProductNotificationSubscription : PersistentSubscriptionProfile
    {
        public ProductNotificationSubscription()
        {
            CreateProfile()
                .SubscribeToAggregateCategory<Product>()
                .HandleEvent<ProductCreatedEvent>();
        }
    }

    public class ProductNotificationSubscriptionHandler :
        ISubscriptionEventHandler<ProductNotificationSubscription, ProductCreatedEvent>
    {
        private readonly ILogger<ProductNotificationSubscriptionHandler> _logger;

        public ProductNotificationSubscriptionHandler(ILogger<ProductNotificationSubscriptionHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleEvent(ProductCreatedEvent @event)
        {
            _logger.LogInformation($"Product Created: ${@event.Name}");
            return Task.CompletedTask;
        }
    }
}
