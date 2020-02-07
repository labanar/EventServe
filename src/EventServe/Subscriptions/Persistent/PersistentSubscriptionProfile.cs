using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Persistent
{
    public abstract class PersistentSubscriptionProfile : IStreamSubscription
    {

        //public PersistentSubscriptionProfile(IPersistentSubscriptionBuilder<TProfile> builder)
        //{
        //    _builder = builder;
        //}


        //protected IPersistentSubscriptionBuilder<TProfile> CreateProfile<TProfile>()
        //    where TProfile: PersistentSubscriptionProfile
        //{
        //    var closedType = typeof(IPersistentSubscriptionBuilder<>).MakeGenericType(typeof(TProfile));

        //}

        //public void Build()
        //{
        //    var subscription = _builder.Build();
        //}
    }
}
