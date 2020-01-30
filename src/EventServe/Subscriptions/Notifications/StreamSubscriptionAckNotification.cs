using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Notifications
{
    public sealed class StreamSubscriptionAckNotification : INotification
    {
        public Guid SubscriptionId { get; }
        public Event Event { get; }
        private StreamSubscriptionAckNotification() { }

        public StreamSubscriptionAckNotification(Guid subscriptionId, Event @event)
        {
            SubscriptionId = subscriptionId;
            Event = @event;
        }
    }
}
