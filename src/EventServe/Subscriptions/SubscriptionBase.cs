using EventServe.Subscriptions.Domain.Enums;
using System;

namespace EventServe.Subscriptions
{
    public class SubscriptionBase
    {
        public Guid SubscriptionId { get; set; }
        public string Name { get; set; }
        public bool Connected { get; set; }
        public SubscriptionType Type { get; set; }
    }
}
