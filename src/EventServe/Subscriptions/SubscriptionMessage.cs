using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions
{
    public class SubscriptionMessage
    {
        public SubscriptionMessage(Guid eventId, string streamId, string type, Func<Event> @event)
        {
            EventId = eventId;
            SourceStreamId = streamId;
            Type = type;
            _event = new Lazy<Event>(@event);
        }

        public Guid EventId { get;}
        public string SourceStreamId { get; }
        public string Type { get; }
        public Event Event => _event.Value;

        private Lazy<Event> _event;
    }
}
