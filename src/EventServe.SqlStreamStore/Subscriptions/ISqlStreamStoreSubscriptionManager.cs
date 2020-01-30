using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public interface ISqlStreamStoreSubscriptionManager
    {
        Task CreateStreamSubscription(Guid subscriptionId);
        Task PersistAcknowledgement<T>(Guid subscriptionId, T @event) where T : Event;
        Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId);
    }
}
