using System;

namespace EventServe.Subscriptions.Persistent
{
    public class PersistentStreamSubscriptionConnectionSettings
    {
        public PersistentStreamSubscriptionConnectionSettings(Guid subscriptionId, string subscriptionName, StreamId streamId, string aggregateType)
        {
            SubscriptionId = subscriptionId;
            SubscriptionName = subscriptionName;
            StreamId = streamId;
            AggregateType = aggregateType;
        }

        public Guid SubscriptionId { get; }
        public string SubscriptionName { get; }
        public StreamId StreamId { get; } 
        public string AggregateType { get; }
    }
}
