using EventServe.Subscriptions.Domain.Enums;
using System;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionCreatedEvent : Event
    {
        public SubscriptionCreatedEvent() { }
        public SubscriptionCreatedEvent(Guid subscriptionId, string name, string streamId, SubscriptionType subscriptionType) : base(Guid.Empty, true)
        {
            SubscriptionId = subscriptionId;
            StreamId = streamId;
            Name = name;
            Type = subscriptionType;
        }

        public Guid SubscriptionId { get; set; }
        public string Name { get; set; }
        public string StreamId { get; set; }
        public SubscriptionType Type { get; set; }
    }
}
