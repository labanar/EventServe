using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public class SubscriptionObserver<TProfile, TEvent> : IObserver<Event>
        where TProfile : ISubscriptionProfile
        where TEvent : Event
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionObserver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

            try
            {
                var worker = Task.Factory
                .StartNew(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetService<ISubscriptionEventHandler<TProfile, TEvent>>();
                        if (handler == null)
                            return;

                        await handler.HandleEvent(typedEvent);
                    }
                });

                worker.Wait();
            }
            catch (AggregateException ae)
            {
                //Check if the task threw any exceptions that we're concerned with
                foreach (var e in ae.InnerExceptions)
                {
                    throw;
                }
            }
        }
    }
}
