using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionStartedEvent : Event
    {
        public SubscriptionStartedEvent() { }

        public SubscriptionStartedEvent(Guid susbcriptionId) : base(Guid.Empty, true)
        {
            SubscriptionId = susbcriptionId;
        }

        public Guid SubscriptionId { get; set; }
    }
}
