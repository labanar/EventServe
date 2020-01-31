using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreSubscriptionAggregate : AggregateRoot
    {
        public override Guid Id => _id;
        public long? Position => _version;

        private Guid _id;
        private string _streamId;
        private long? _version;
        private HashSet<Guid> _acknowledgedEventIds = new HashSet<Guid>();

        private SqlStreamStoreSubscriptionAggregate() { }
        public SqlStreamStoreSubscriptionAggregate(Guid subscriptionId, string streamId)
        {
            ApplyChange(new SqlStreamStoreSubscriptionCreatedEvent(subscriptionId, streamId));
        }
        public void AcknowledgeEvent(Guid eventId)
        {
            ApplyChange(new SqlStreamStoreSubscriptionAcknowledgeEvent(_id, eventId));
        }

        private void Apply(SqlStreamStoreSubscriptionCreatedEvent @event)
        {
            _id = @event.AggregateId;
            _streamId = @event.StreamId;
        }
        private void Apply(SqlStreamStoreSubscriptionAcknowledgeEvent @event)
        {
            _version += 1;
            _acknowledgedEventIds.Add(@event.EventId);
        }
    }
}
