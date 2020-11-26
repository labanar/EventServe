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

        public bool Disabled { get; }

        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;

        public PersistentSubscriptionProfile(bool disabled = false)
        {
            Disabled = disabled;
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
