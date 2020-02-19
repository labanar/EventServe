using System;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public class SubscriptionObserver<TProfile, TEvent> : IObserver<Event>
        where TProfile : ISubscriptionProfile
        where TEvent : Event
    {
        private readonly ISusbcriptionHandlerResolver _resolver;

        public SubscriptionObserver(ISusbcriptionHandlerResolver resolver)
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
            if (!(@event is TEvent typedEvent))
                return;

            var handlerTask = _resolver.Resolve<TProfile, TEvent>();
            Task.WaitAll(handlerTask);

            var handler = handlerTask.Result;
            if (handler == null)
                return;

            handler.HandleEvent(typedEvent).Wait();
        }
    }
}
