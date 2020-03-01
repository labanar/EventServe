using EventServe.Subscriptions.Persistent;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventServe.Projections.Partitioned
{
    public class PartitionedProjectionResetObserver<TProjection> : IObserver<PersistentSubscriptionResetEvent>
        where TProjection : PartitionedProjection
    {
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionResetObserver(IServiceProvider serviceProvider)
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var stateRepository = scope.ServiceProvider.GetRequiredService<IPartitionedProjectionStateRepository>();
                stateRepository.ResetState<TProjection>().Wait();
            }      
        }
    }
}
