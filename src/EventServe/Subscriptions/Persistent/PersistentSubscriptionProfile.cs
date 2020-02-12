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
        public HashSet<Type> SubscribedEvents => _eventTypes;

        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;

        public PersistentSubscriptionProfile()
        {
            _subscriptionFilterBuilder = new SubscriptionFilterBuilder();
            _eventTypes = new HashSet<Type>();
        }

        public IPersistentSubscriptionProfileExpression CreateProfile()
        {
            var expression = new PersistentSubcriptionBuilderExpression(_subscriptionFilterBuilder, _eventTypes);
            return expression;
        }
    }
}
