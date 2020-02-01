using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class SqlStreamStoreSubscriptionPosition
    {
        public Guid SubscriptionId { get; set; }
        public string StreamId { get; set; }
    }
}
