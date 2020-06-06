using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public interface ITransientSubscriptionPositionExpression
    {
        ITransientSubscriptionStreamExpression StartFromBeginning();
        ITransientSubscriptionStreamExpression StartFromEnd();
    }
}
