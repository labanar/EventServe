using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public class SubscriptionObserver<TProfile, TEvent> : IObserver<Event>
        where TProfile : ISubscriptionProfile
        where TEvent : Event
    {
        private readonly ISusbcriptionProfileHandlerResolver _resolver;

        public SubscriptionObserver(ISusbcriptionProfileHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(Event @event)
        {
            //TODO - this does not feel right
            if (!(@event is TEvent))
                return;

            var handlerTask = _resolver.Resolve<TProfile, TEvent>();
            Task.WaitAll(handlerTask);

            var handler = handlerTask.Result;
            if (handler == null)
                return;

            handler.HandleEvent(@event as TEvent).Wait();
        }
    }
}
