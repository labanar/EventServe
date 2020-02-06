using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionStoppedEvent : Event
    {
        public SubscriptionStoppedEvent() { }

        public SubscriptionStoppedEvent(Guid susbcriptionId, string reason, Exception exception = null) : base(Guid.Empty, true)
        {
            SubscriptionId = susbcriptionId;
        }

        public Guid SubscriptionId { get; set; }
        public string Reason { get; set; }
        public Exception Exception { get; set; }
    }
}
