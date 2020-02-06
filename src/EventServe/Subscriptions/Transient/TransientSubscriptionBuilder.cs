using System;

namespace EventServe.Subscriptions
{
    public interface ITransientSubscriptionBuilder<TSubscription>
        where TSubscription : StreamSubscription
    {
        TransientSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>() where T : AggregateRoot;
        TransientSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id) where T : AggregateRoot;
        TransientSubscriptionBuilder<TSubscription> SubscribeToStream(string id);
        TransientSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event;
        TransientSubscriptionBuilder<TSubscription> StartAtBeginningOfStream();
        TransientSubscriptionBuilder<TSubscription> StartAtEndOfStream();
    }

    public class TransientSubscriptionBuilder<TSubscription> : ITransientSubscriptionBuilder<TSubscription>
        where TSubscription : StreamSubscription
    {
        private readonly Guid _id;
        private readonly ISubscriptionRootManager _subscriptionRootManager;
        private readonly ITransientStreamSubscription _subscription;
        private readonly IServiceProvider _serviceProvider;
        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder = new SubscriptionFilterBuilder();

        //TODO - Enum?
        public int _startPosition = -1; //END

        public TransientSubscriptionBuilder(
            ITransientStreamSubscription transientStreamSubscription,
            ISubscriptionRootManager subscriptionRootManager,
            IServiceProvider serviceProvider
            )
        {
            _id = Guid.NewGuid();
            _subscriptionRootManager = subscriptionRootManager;
            _subscription = transientStreamSubscription;
            _serviceProvider = serviceProvider;
        }

        public TransientSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>()
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregateCategory<T>();
            return this;
        }

        public TransientSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id)
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregate<T>(id);
            return this;
        }

        public TransientSubscriptionBuilder<TSubscription> SubscribeToStream(string id)
        {
            _subscriptionFilterBuilder.SubscribeToStream(id);
            return this;
        }

        public TransientSubscriptionBuilder<TSubscription> StartAtBeginningOfStream()
        {
            _startPosition = 0;
            return this;
        }

        public TransientSubscriptionBuilder<TSubscription> StartAtEndOfStream()
        {
            _startPosition = -1;
            return this;
        }

        public TransientSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event
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

            //TODO - return subscription base here
        }


    }
}
