using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public class SubscriptionMessageObserver<TProfile, TEvent> : IObserver<SubscriptionMessage>
        where TProfile: ISubscriptionProfile
        where TEvent: Event
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStreamFilter _filter;

        public SubscriptionMessageObserver(
            IServiceProvider serviceProvider,
            IStreamFilter filter)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
        }

        public void OnCompleted() { }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(SubscriptionMessage value)
        {
            //Check if this event passes through the filter
            if (_filter != null && !_filter.DoesEventPassFilter(value.Type, value.SourceStreamId))
                return;

            if (!(value.Event is TEvent typedEvent))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {

                var handler = scope.ServiceProvider.GetService<ISubscriptionEventHandler<TProfile, TEvent>>();
                if (handler == null)
                    return;

                handler.HandleEvent(typedEvent).Wait();
            }
        }
    }
}
