using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientStreamSubscriptionConnectionSettings
    {
        public TransientStreamSubscriptionConnectionSettings(Guid subscriptionId, string subscriptionName, StreamPosition streamPosition, StreamId streamId, string aggregateType)
        {
            SubscriptionId = subscriptionId;
            StreamPosition = streamPosition;
            StreamId = streamId;
            AggregateType = aggregateType;
            SubscriptionName = subscriptionName;
        }

        public Guid SubscriptionId { get; }
        public string SubscriptionName { get; }
        public StreamPosition StreamPosition { get; }
        public StreamId StreamId { get; }
        public string AggregateType { get; }
    }
}
