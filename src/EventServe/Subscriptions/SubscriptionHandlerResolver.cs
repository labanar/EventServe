using System;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionHandlerResolver
    {
        Task<IStreamSubscriptionEventHandler<TSubscription, TEvent>> Resolve<TSubscription, TEvent>()
            where TSubscription : StreamSubscription
            where TEvent : Event;
    }

    public class SubscriptionHandlerResolver: ISubscriptionHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IStreamSubscriptionEventHandler<TSubscription, TEvent>> Resolve<TSubscription, TEvent>()
            where TSubscription : StreamSubscription
            where TEvent : Event
        {
            var handler = (IStreamSubscriptionEventHandler<TSubscription, TEvent>)_serviceProvider.GetService(typeof(IStreamSubscriptionEventHandler<TSubscription, TEvent>));
            return Task.FromResult(handler);
        }
    }
}
