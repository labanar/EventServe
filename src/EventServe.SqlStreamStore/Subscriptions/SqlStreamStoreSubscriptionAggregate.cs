using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamSubscriptionManagerAggregate : AggregateRoot
    {
        public override Guid Id => Guid.Empty;

        public List<SqlStreamStoreSubscriptionPosition> Subscriptions = new List<SqlStreamStoreSubscriptionPosition>();

        public SqlStreamSubscriptionManagerAggregate() { }

        public void CreateStreamSubscription(Guid subscriptionId, string streamId)
        {
            ApplyChange(new SqlStreamStoreSubscriptionCreatedEvent(subscriptionId, streamId));
        }

        private void Apply(SqlStreamStoreSubscriptionCreatedEvent @event)
        {
            Subscriptions.Add(new SqlStreamStoreSubscriptionPosition
            {
                SubscriptionId = @event.SubscriptionId,
                StreamId = @event.StreamId
            });
        }
    }
}
