using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public abstract class TransientSubscriptionProfile<TProfile> : IStreamSubscription
        where TProfile : TransientSubscriptionProfile<TProfile>
    {
        private readonly ITransientSubscriptionBuilder<TProfile> _builder;

        public TransientSubscriptionProfile(ITransientSubscriptionBuilder<TProfile> builder)
        {
            _builder = builder;
        }
    }
}
