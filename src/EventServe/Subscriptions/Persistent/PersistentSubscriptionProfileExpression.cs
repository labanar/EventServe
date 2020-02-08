using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Persistent
{
    public class PersistentSubcriptionBuilderExpression : 
        IPersistentSubscriptionProfileExpression, 
        IPersistentSubscriptionHandlerExpression
    {
        private readonly SubscriptionFilterBuilder _filterBuilder;
        private HashSet<Type> _subscribedEventTypes = new HashSet<Type>();

        public PersistentSubcriptionBuilderExpression(SubscriptionFilterBuilder filterBuilder, HashSet<Type> subscribedEvents)
        {
            _filterBuilder = filterBuilder;
            _subscribedEventTypes = subscribedEvents;
        }

        public IPersistentSubscriptionHandlerExpression SubscribeToAggregate<T>(Guid id) where T : AggregateRoot
        {
            _filterBuilder.SubscribeToAggregate<T>(id);
            return this;
        }

        public IPersistentSubscriptionHandlerExpression SubscribeToAggregateCategory<T>() where T : AggregateRoot
        {
            _filterBuilder.SubscribeToAggregateCategory<T>();
            return this;
        }

        public IPersistentSubscriptionHandlerExpression HandleEvent<T>() where T : Event
        {
            _subscribedEventTypes.Add(typeof(T));
            return this;
        }
    }
}
