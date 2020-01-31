using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreSubscriptionCreatedEvent : Event
    {
        public SqlStreamStoreSubscriptionCreatedEvent() { }
        public SqlStreamStoreSubscriptionCreatedEvent(Guid subscriptionId, string streamId) : base(subscriptionId)
        {
            StreamId = streamId;
        }

        public string StreamId { get; set; }
    }
}
