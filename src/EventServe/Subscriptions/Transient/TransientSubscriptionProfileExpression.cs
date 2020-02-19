using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientSubscriptionProfileExpression :
        ITransientSubscriptionPositionExpression,
        ITransientSubscriptionHandlerExpression,
        ITransientSubscriptionStreamExpression

    {
        private readonly SubscriptionFilterBuilder _filterBuilder;
        private HashSet<Type> _subscribedEventTypes = new HashSet<Type>();
        private StreamPosition _position = StreamPosition.End;

        public TransientSubscriptionProfileExpression(SubscriptionFilterBuilder filterBuilder, HashSet<Type> subscribedEvents, StreamPosition position)
        {
            _filterBuilder = filterBuilder;
            _subscribedEventTypes = subscribedEvents;
            _position = position;
        }

        public ITransientSubscriptionStreamExpression StartFromBeginning()
        {
            _position.SetPositionToBeginning();
            return this;
        }

        public ITransientSubscriptionStreamExpression StartFromEnd()
        {
            _position.SetPostionToEnd();
            return this;
        }

        public ITransientSubscriptionHandlerExpression SubscribeToAggregate<T>(Guid id) where T : AggregateRoot
        {
            _filterBuilder.SubscribeToAggregate<T>(id);
            return this;
        }

        public ITransientSubscriptionHandlerExpression SubscribeToAggregateCategory<T>() where T : AggregateRoot
        {
            _filterBuilder.SubscribeToAggregateCategory<T>();
            return this;
        }

        public ITransientSubscriptionHandlerExpression HandleEvent<T>() where T : Event
        {
            _subscribedEventTypes.Add(typeof(T));
            _filterBuilder.HandleEvent<T>();
            return this;
        }
    }
}
