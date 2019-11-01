using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IStreamSubscriptionEventHandler<T>
        where T: EventStreamSubscription
    {
        Task Handle(Event @event);
    }
}
