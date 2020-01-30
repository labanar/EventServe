using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Subscriptions.Notifications
{
    public sealed class StreamSubscriptionEventNotification : INotification
    {
        public Guid SubscriptionId { get; }
        public Event Event { get; }
        public  Func<Event, Task> AcknowledgementCallback { get; }

        private StreamSubscriptionEventNotification() { }

        public StreamSubscriptionEventNotification(Guid subscriptionId, Event @event, Func<Event, Task> acknowledgementCallback)
        {
            SubscriptionId = subscriptionId;
            Event = @event;
            AcknowledgementCallback = acknowledgementCallback;
        }
    }
}
