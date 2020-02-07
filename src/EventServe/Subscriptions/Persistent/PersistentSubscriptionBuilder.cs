using System;

namespace EventServe.Subscriptions.Persistent
{
    public interface IPersistentSubscriptionBuilder<TSubscription>
        where TSubscription : IStreamSubscription
    {
        IPersistentSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>() where T : AggregateRoot;
        IPersistentSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id) where T : AggregateRoot;
        IPersistentSubscriptionBuilder<TSubscription> SubscribeToStream(string id);
        IPersistentSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event;
        IPersistentStreamSubscription Build();
    }

    public class PersistentSubscriptionBuilder<TSubscription> : IPersistentSubscriptionBuilder<TSubscription>
        where TSubscription : IStreamSubscription
    {
        private readonly IPersistentStreamSubscription _subscription;
        private readonly IServiceProvider _serviceProvider;
        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder = new SubscriptionFilterBuilder();

        public PersistentSubscriptionBuilder(
            IPersistentStreamSubscription persistentStreamSubscription,
            IServiceProvider serviceProvider
            )
        {
            _subscription = persistentStreamSubscription;
            _serviceProvider = serviceProvider;
        }

        public IPersistentSubscriptionBuilder<TSubscription> SubscribeToAggregateCategory<T>()
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregateCategory<T>();
            return this;
        }

        public IPersistentSubscriptionBuilder<TSubscription> SubscribeToAggregate<T>(Guid id)
            where T : AggregateRoot
        {
            _subscriptionFilterBuilder.SubscribeToAggregate<T>(id);
            return this;
        }

        public IPersistentSubscriptionBuilder<TSubscription> SubscribeToStream(string id)
        {
            _subscriptionFilterBuilder.SubscribeToStream(id);
            return this;
        }
        public IPersistentSubscriptionBuilder<TSubscription> ListenFor<T>() where T : Event
        {
            //TODO - avoid service locator?
            var resolver = (ISubscriptionHandlerResolver)_serviceProvider.GetService(typeof(ISubscriptionHandlerResolver));
            var observer = new StreamSubscriptionObserver<TSubscription, T>(resolver);
            _subscription.Subscribe(observer);
            return this;
        }

        public IPersistentStreamSubscription Build()
        {
            var filter = _subscriptionFilterBuilder.Build();
            _subscription.Start(typeof(TSubscription).Name, filter).Wait();
            return _subscription;
        }


    }
}
