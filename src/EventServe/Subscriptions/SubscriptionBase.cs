using EventServe.Subscriptions.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions
{
    public class SubscriptionBase
    {
        public Guid SubscriptionId { get; set; }
        public string StreamId { get; set; }
        public string Name { get; set; }
        public bool Connected { get; set; }
        public SubscriptionType Type { get; set; }
    }
}
