using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface IStreamSubscriptionEventHandler<TSubscription,TEvent>
        where TEvent: Event
        where TSubscription: StreamSubscription
    {

        Task HandleEvent(TEvent @event);
    }
}
