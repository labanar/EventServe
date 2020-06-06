using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Persistent
{
    public interface IPersistentSubscriptionHandlerExpression
    {
        IPersistentSubscriptionHandlerExpression HandleEvent<T>() where T : Event;
    }
}
