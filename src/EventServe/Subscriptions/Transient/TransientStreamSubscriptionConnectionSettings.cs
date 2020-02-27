using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientStreamSubscriptionConnectionSettings
    {
        public TransientStreamSubscriptionConnectionSettings(Guid subscriptionId, StreamPosition streamPosition, StreamId streamId, string aggregateType)
        {
            SubscriptionId = subscriptionId;
            StreamPosition = streamPosition;
            StreamId = streamId;
            AggregateType = aggregateType;
        }

        public Guid SubscriptionId { get; }
        public StreamPosition StreamPosition { get; }
        public StreamId StreamId { get; }
        public string AggregateType { get; }
    }
}
