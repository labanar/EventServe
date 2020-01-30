using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public class MsSqlStreamSubscriptionPosition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long? Position { get; set; }
    }
}
