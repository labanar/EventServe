using System;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public class StreamSubscriptionObserver<TSubscription, TEvent> : IObserver<Event>
        where TEvent: Event
        where TSubscription: StreamSubscription
    {
        private readonly ISubscriptionHandlerResolver _resolver;

        public StreamSubscriptionObserver(ISubscriptionHandlerResolver resolver)
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

            var handlerTask = _resolver.Resolve<TSubscription, TEvent>();
            Task.WaitAll(handlerTask);

            var handler = handlerTask.Result;
            if (handler == null)
                return;

            handler.HandleEvent(@event as TEvent).Wait();
        }
    }
}
