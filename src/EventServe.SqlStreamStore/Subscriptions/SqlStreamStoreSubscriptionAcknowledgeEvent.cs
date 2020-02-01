using System;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreSubscriptionAcknowledgeEvent : Event
    {
        public SqlStreamStoreSubscriptionAcknowledgeEvent() { }
        public SqlStreamStoreSubscriptionAcknowledgeEvent(Guid subscriptionId, Guid eventId) : base(subscriptionId)
        {
            AcknowledgedEventId = eventId;
        }

        public Guid AcknowledgedEventId { get; set; }

    }
}
