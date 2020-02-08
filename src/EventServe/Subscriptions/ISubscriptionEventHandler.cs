using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionEventHandler<TProfile, TEvent>
        where TProfile: ISubscriptionProfile
        where TEvent: Event
    {
        Task HandleEvent(TEvent @event);
    }
}
