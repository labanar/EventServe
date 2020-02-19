using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientStreamSubscriptionConnectionSettings
    {
        public TransientStreamSubscriptionConnectionSettings(StreamPosition streamPosition, IStreamFilter filter)
        {
            StreamPosition = streamPosition;
            Filter = filter;
        }

        public StreamPosition StreamPosition { get; }
        public IStreamFilter Filter { get; }
    }
}
