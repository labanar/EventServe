using System;
using System.Collections.Generic;

namespace EventServe.Subscriptions.Persistent
{

    public interface IPersistentSubscriptionProfile
    {
        SubscriptionFilter Filter { get; }
        HashSet<Type> SubscribedEvents { get; }
        IPersistentSubscriptionProfileExpression CreateProfile();
    }

    public abstract class PersistentSubscriptionProfile: IPersistentSubscriptionProfile, ISubscriptionProfile
    {
        public SubscriptionFilter Filter => _subscriptionFilterBuilder.Build();
        public HashSet<Type> SubscribedEvents => _subscribedEvents;

        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder;
        private readonly HashSet<Type> _subscribedEvents;

        public PersistentSubscriptionProfile()
        {
            _subscriptionFilterBuilder = new SubscriptionFilterBuilder();
            _subscribedEvents = new HashSet<Type>();
        }

        public IPersistentSubscriptionProfileExpression CreateProfile()
        {
            var expression = new PersistentSubcriptionBuilderExpression(_subscriptionFilterBuilder, _subscribedEvents);
            return expression;
        }
    }
}
