using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions
{
    public class SubscriptionFilterBuilder
    {
        private StreamId _streamId;
        private Type _aggregateType;
        private readonly HashSet<string> _streamExpressions = new HashSet<string>();
        private readonly HashSet<Type> _eventTypes = new HashSet<Type>();

        public SubscriptionFilterBuilder() { }

        public SubscriptionFilterBuilder SubscribeToAggregate<T>(Guid id)
            where T : AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot subscribe to more than one aggregate type per subscription.");

            _aggregateType = typeof(T);
            if (_streamId == null)
                _streamId = new StreamId($"{typeof(T).Name.ToUpper()}-{id}");

            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{id}$");
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToAggregateCategory<T>()
         where T : AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot subscribe to more than one aggregate type per subscription.");

            _streamId = null;
            _aggregateType = typeof(T);
            var guidExpression = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{guidExpression}$");
            return this;
        }

        public SubscriptionFilterBuilder HandleEvent<T>()
                 where T : Event
        {
            _eventTypes.Add(typeof(T));
            return this;
        }

        public SubscriptionFilter Build()
        {
            //TODO - Argument check
            if (_streamId != null)
                return new SubscriptionFilter(_streamId, _streamExpressions, _eventTypes);
            else
                return new SubscriptionFilter(_aggregateType, _streamExpressions, _eventTypes);
        }
    }
}
