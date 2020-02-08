using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public interface ITransientSubscriptionStreamExpression
    {
        ITransientSubscriptionHandlerExpression SubscribeToAggregate<T>(Guid id) where T : AggregateRoot;
        ITransientSubscriptionHandlerExpression SubscribeToAggregateCategory<T>() where T : AggregateRoot;
        ITransientSubscriptionHandlerExpression SubscribeToStream(string streamId);
    }
}
