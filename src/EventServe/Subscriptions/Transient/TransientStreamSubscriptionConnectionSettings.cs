using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientStreamSubscriptionConnectionSettings
    {
        public TransientStreamSubscriptionConnectionSettings(StreamPosition streamPosition, StreamId streamId, string aggregateType)
        {
            StreamPosition = streamPosition;
            StreamId = streamId;
            AggregateType = aggregateType;
        }

        public StreamPosition StreamPosition { get; }

        public StreamId StreamId { get; }
        public string AggregateType { get; }
    }
}
