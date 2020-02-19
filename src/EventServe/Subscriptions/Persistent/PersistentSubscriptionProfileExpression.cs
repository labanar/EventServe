using System;
using System.Collections.Generic;

namespace EventServe.Subscriptions.Persistent
{
    public class PersistentSubcriptionBuilderExpression : 
        IPersistentSubscriptionProfileExpression, 
        IPersistentSubscriptionHandlerExpression
    {
        private readonly SubscriptionFilterBuilder _filterBuilder;
        private readonly HashSet<Type> _eventTypes;

        public PersistentSubcriptionBuilderExpression(SubscriptionFilterBuilder filterBuilder, HashSet<Type> eventTypes)
        {
            _filterBuilder = filterBuilder;
            _eventTypes = eventTypes;
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
            _filterBuilder.HandleEvent<T>();
            _eventTypes.Add(typeof(T));
            return this;
        }
    }
}
