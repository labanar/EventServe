using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreSubscriptionCreatedEvent : Event
    {
        public SqlStreamStoreSubscriptionCreatedEvent() { }
        public SqlStreamStoreSubscriptionCreatedEvent(Guid subscriptionId, string streamId) : base(Guid.Empty, true)
        {
            StreamId = streamId;
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; set; }
        public string StreamId { get; set; }
    }
}
