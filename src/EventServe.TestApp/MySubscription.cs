using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    public class MySubscriptionProfile6 : PersistentSubscriptionProfile
    {
        public MySubscriptionProfile6()
        {
            CreateProfile()
                .SubscribeToAggregateCategory<DummyAggregate>()
                .HandleEvent<DummyUrlChangedEvent>()
                .HandleEvent<DummyNameChangedEvent>();
        }

        public class Handler :
            ISubscriptionEventHandler<MySubscriptionProfile6, DummyUrlChangedEvent>,
            ISubscriptionEventHandler<MySubscriptionProfile6, DummyNameChangedEvent>
        {
            private readonly ILogger<Handler> _logger;

            public Handler(ILogger<Handler> logger)
            {
                _logger = logger;
            }

            public Task HandleEvent(DummyUrlChangedEvent @event)
            {
                _logger.LogInformation($"Event received: {@event.GetType().Name} [{@event.EventId}]");
                return Task.CompletedTask;
            }

            public Task HandleEvent(DummyNameChangedEvent @event)
            {
                _logger.LogInformation($"Event received: {@event.GetType().Name} [{@event.EventId}]");
                return Task.CompletedTask;
            }
        }
    }

    //public class MyTransientSubscription : TransientSubscriptionProfile
    //{
    //    public MyTransientSubscription()
    //    {
    //        CreateProfile()
    //            .StartFromBeginning()
    //            .SubscribeToAggregateCategory<DummyAggregate>()
    //            .HandleEvent<DummyUrlChangedEvent>()
    //            .HandleEvent<DummyNameChangedEvent>();
    //    }

    //    public class Handler :
    //        ISubscriptionEventHandler<MyTransientSubscription, DummyUrlChangedEvent>,
    //        ISubscriptionEventHandler<MyTransientSubscription, DummyNameChangedEvent>
    //    {
    //        private readonly ILogger<Handler> _logger;

    //        public Handler(ILogger<Handler> logger)
    //        {
    //            _logger = logger;
    //        }

    //        public Task HandleEvent(DummyUrlChangedEvent @event)
    //        {
    //            _logger.LogInformation($"Event received: {@event.GetType().Name} [{@event.EventId}]");
    //            return Task.CompletedTask;
    //        }

    //        public Task HandleEvent(DummyNameChangedEvent @event)
    //        {
    //            _logger.LogInformation($"Event received: {@event.GetType().Name} [{@event.EventId}]");
    //            return Task.CompletedTask;
    //        }
    //    }
    //}
}
