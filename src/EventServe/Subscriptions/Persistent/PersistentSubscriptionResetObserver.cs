using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EventServe.Subscriptions.Persistent
{
    public class PersistentSubscriptionResetEvent { }

    public class PersistentSubscriptionResetObserver<TProfile> : IObserver<PersistentSubscriptionResetEvent>
        where TProfile: PersistentSubscriptionProfile
    {
        private readonly IServiceProvider _serviceProvider;

        public PersistentSubscriptionResetObserver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnCompleted() { }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(PersistentSubscriptionResetEvent value)
        {
            try
            {
                var worker = Task.Factory
                .StartNew(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<IPersistentSubscriptionResetHandler<TProfile>>();
                        await handler.HandleReset();
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
