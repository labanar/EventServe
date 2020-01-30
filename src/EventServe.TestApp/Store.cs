using EventServe.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    public static class Store
    {
        public static IPersistentSreamSubscription Subscription { get; set; }
    }
}
