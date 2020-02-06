using EventServe.Subscriptions;
using EventServe.Subscriptions.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    //Name of this doesn't matter so much
    public class MyStreamSubscription : StreamSubscription { }

    public class MyStreamSubscriptionHandler : 
        IStreamSubscriptionEventHandler<MyStreamSubscription, DummyCreatedEvent>,
        IStreamSubscriptionEventHandler<MyStreamSubscription, DummyUrlChangedEvent>,
        IStreamSubscriptionEventHandler<MyStreamSubscription, DummyNameChangedEvent>

    {
        private readonly ILogger<MyStreamSubscription> _logger;

        public MyStreamSubscriptionHandler(ILogger<MyStreamSubscription> logger)
        {
            _logger = logger;
        }

        public async Task HandleEvent(DummyCreatedEvent @event)
        {
            //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
        }

        public async Task HandleEvent(DummyUrlChangedEvent @event)
        {
            //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
        }

        public async Task HandleEvent(DummyNameChangedEvent @event)
        {
            //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
        }
    }
}
