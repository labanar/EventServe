using System;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionDeletedEvent : Event
    {
        public SubscriptionDeletedEvent() { }

        public SubscriptionDeletedEvent(Guid susbcriptionId) : base(Guid.Empty, true)
        {
            SubscriptionId = susbcriptionId;
        }

        public Guid SubscriptionId { get; set; }
    }
}