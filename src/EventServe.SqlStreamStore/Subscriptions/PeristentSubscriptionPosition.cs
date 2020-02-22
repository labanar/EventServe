using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class PeristentSubscriptionPosition
    {
        public Guid SubscriptionId { get; set; }
        public long? Position { get; set; }
    }
}
