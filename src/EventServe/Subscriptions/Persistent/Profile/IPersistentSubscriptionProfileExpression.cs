using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Persistent
{

    public interface IPersistentSubscriptionProfileExpression
    {
        IPersistentSubscriptionHandlerExpression SubscribeToAggregate<T>(Guid id) where T : AggregateRoot;
        IPersistentSubscriptionHandlerExpression SubscribeToAggregateCategory<T>() where T : AggregateRoot;
    }

}
