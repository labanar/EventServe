using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    //public class MySubscriptionProfile : PersistentSubscriptionProfile<MySubscriptionProfile>
    //{
    //    public MySubscriptionProfile(IPersistentSubscriptionBuilder<MySubscriptionProfile> builder) : base(builder)
    //    {
    //        builder
    //            .SubscribeToAggregateCategory<DummyAggregate>()
    //            .ListenFor<DummyUrlChangedEvent>()
    //            .ListenFor<DummyNameChangedEvent>()
    //            .Build();
    //    }

    //    public class Handler : 
    //        IStreamSubscriptionEventHandler<MySubscriptionProfile, DummyUrlChangedEvent>,
    //        IStreamSubscriptionEventHandler<MySubscriptionProfile, DummyNameChangedEvent>
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


    //Name of this doesn't matter so much
    //public class MyStreamSubscription3 : IStreamSubscription { }

    //public class MyStreamSubscription2 : IStreamSubscription { }

    //public class MyStreamSubscription2Handler :
    //IStreamSubscriptionEventHandler<MyStreamSubscription2, DummyCreatedEvent>,
    //IStreamSubscriptionEventHandler<MyStreamSubscription2, DummyUrlChangedEvent>,
    //IStreamSubscriptionEventHandler<MyStreamSubscription2, DummyNameChangedEvent>

    //{
    //    private readonly ILogger<MyStreamSubscription> _logger;

    //    public MyStreamSubscription2Handler(ILogger<MyStreamSubscription> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public async Task HandleEvent(DummyCreatedEvent @event)
    //    {
    //        _logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }

    //    public async Task HandleEvent(DummyUrlChangedEvent @event)
    //    {
    //        _logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }

    //    public async Task HandleEvent(DummyNameChangedEvent @event)
    //    {
    //        _logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }
    //}

    //public class MyStreamSubscription : IStreamSubscription { }

    //public class MyStreamSubscriptionHandler : 
    //    IStreamSubscriptionEventHandler<MyStreamSubscription, DummyCreatedEvent>,
    //    IStreamSubscriptionEventHandler<MyStreamSubscription, DummyUrlChangedEvent>,
    //    IStreamSubscriptionEventHandler<MyStreamSubscription, DummyNameChangedEvent>

    //{
    //    private readonly ILogger<MyStreamSubscription> _logger;

    //    public MyStreamSubscriptionHandler(ILogger<MyStreamSubscription> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public async Task HandleEvent(DummyCreatedEvent @event)
    //    {
    //        //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }

    //    public async Task HandleEvent(DummyUrlChangedEvent @event)
    //    {
    //        //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }

    //    public async Task HandleEvent(DummyNameChangedEvent @event)
    //    {
    //        //_logger.LogInformation($"EVENT RECEIVED: {@event.EventId}");
    //    }
    //}
}
