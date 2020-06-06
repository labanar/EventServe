using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public interface ITransientSubscriptionHandlerExpression
    {
        ITransientSubscriptionHandlerExpression HandleEvent<T>() where T : Event;
    }
}
