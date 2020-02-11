using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public class TransientStreamSubscriptionConnectionSettings
    {
        public TransientStreamSubscriptionConnectionSettings(int startPosition, SubscriptionFilter filter)
        {
            StartPosition = startPosition;
            Filter = filter;
        }

        public int StartPosition { get; }
        public SubscriptionFilter Filter { get; }
    }
}
