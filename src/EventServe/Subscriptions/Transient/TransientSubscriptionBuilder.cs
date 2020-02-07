using System;

namespace EventServe.Subscriptions
{
    public interface ITransientSubscriptionBuilder<TSubscription>
        where TSubscription : IStreamSubscription
    {
        ITransientSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>() where T : AggregateRoot;
        ITransientSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id) where T : AggregateRoot;
        ITransientSubscriptionBuilder<TSubscription> SubscribeToStream(string id);
        ITransientSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event;
        ITransientSubscriptionBuilder<TSubscription> StartAtBeginningOfStream();
        ITransientSubscriptionBuilder<TSubscription> StartAtEndOfStream();
        ITransientStreamSubscription Build();
    }

    public class TransientSubscriptionBuilder<TSubscription> : ITransientSubscriptionBuilder<TSubscription>
        where TSubscription : IStreamSubscription
    {
        private readonly Guid _id;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ITransientStreamSubscription _subscription;
        private readonly IServiceProvider _serviceProvider;
        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder = new SubscriptionFilterBuilder();

        //TODO - Enum?
        public int _startPosition = -1; //END

        public TransientSubscriptionBuilder(
            ITransientStreamSubscription transientStreamSubscription,
            ISubscriptionManager subscriptionManager,
            IServiceProvider serviceProvider
            )
        {
            _id = Guid.NewGuid();
            _subscriptionManager = subscriptionManager;
            _subscription = transientStreamSubscription;
            _serviceProvider = serviceProvider;
        }

        public ITransientSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>()
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregateCategory<T>();
            return this;
        }

        public ITransientSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id)
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregate<T>(id);
            return this;
        }

        public ITransientSubscriptionBuilder<TSubscription> SubscribeToStream(string id)
        {
            _subscriptionFilterBuilder.SubscribeToStream(id);
            return this;
        }

        public ITransientSubscriptionBuilder<TSubscription> StartAtBeginningOfStream()
        {
            _startPosition = 0;
            return this;
        }

        public ITransientSubscriptionBuilder<TSubscription> StartAtEndOfStream()
        {
            _startPosition = -1;
            return this;
        }

        public ITransientSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event
        {
            //TODO - avoid service locator?
            var resolver = (ISubscriptionHandlerResolver)_serviceProvider.GetService(typeof(ISubscriptionHandlerResolver));
            var observer = new StreamSubscriptionObserver<TSubscription, T>(resolver);
            _subscription.Subscribe(observer);
            return this;
        }

        public ITransientStreamSubscription Build()
        {
            var filter = _subscriptionFilterBuilder.Build();
            _subscription.Start(_startPosition, filter).Wait();

            return _subscription;
        }
    }
}
