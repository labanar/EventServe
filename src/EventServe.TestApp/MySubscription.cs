using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    public class MySubscriptionProfile : PersistentSubscriptionProfile<MySubscriptionProfile>
    {
        public MySubscriptionProfile(IPersistentSubscriptionBuilder<MySubscriptionProfile> builder) : base(builder)
        {
            builder
                .SubscribeToAggregateCategory<DummyAggregate>()
                .ListenFor<DummyUrlChangedEvent>()
                .ListenFor<DummyNameChangedEvent>()
                .Build();
        }

        public class Handler :
            IStreamSubscriptionEventHandler<MySubscriptionProfile, DummyUrlChangedEvent>,
            IStreamSubscriptionEventHandler<MySubscriptionProfile, DummyNameChangedEvent>
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

    public class MySubscriptionProfile2 : PersistentSubscriptionProfile<MySubscriptionProfile2>
    {
        public MySubscriptionProfile2(IPersistentSubscriptionBuilder<MySubscriptionProfile2> builder) : base(builder)
        {
            builder
                .SubscribeToAggregateCategory<DummyAggregate>()
                .ListenFor<DummyUrlChangedEvent>()
                .ListenFor<DummyNameChangedEvent>()
                .Build();
        }

        public class Handler :
            IStreamSubscriptionEventHandler<MySubscriptionProfile2, DummyUrlChangedEvent>,
            IStreamSubscriptionEventHandler<MySubscriptionProfile2, DummyNameChangedEvent>
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
}
