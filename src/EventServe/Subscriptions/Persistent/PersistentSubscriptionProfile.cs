using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Persistent
{
    public abstract class PersistentSubscriptionProfile<TProfile> : IStreamSubscription
        where TProfile : PersistentSubscriptionProfile<TProfile>
    {
        private readonly IPersistentSubscriptionBuilder<TProfile> _builder;

        public PersistentSubscriptionProfile(IPersistentSubscriptionBuilder<TProfile> builder)
        {
            _builder = builder;
        }
    }
}
