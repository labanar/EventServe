using System;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISusbcriptionHandlerResolver
    {
        Task<ISubscriptionEventHandler<TProfile, TEvent>> Resolve<TProfile, TEvent>()
            where TProfile : ISubscriptionProfile
            where TEvent : Event;
    }

    public class SubscriptionHandlerResolver : ISusbcriptionHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<ISubscriptionEventHandler<TProfile, TEvent>> Resolve<TProfile, TEvent>()
            where TProfile : ISubscriptionProfile
            where TEvent : Event
        {
            var handler = (ISubscriptionEventHandler<TProfile, TEvent>)_serviceProvider
                .GetService(typeof(ISubscriptionEventHandler<TProfile, TEvent>));

            return Task.FromResult(handler);
        }
    }
}
