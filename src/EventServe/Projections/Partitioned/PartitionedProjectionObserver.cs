using EventServe.Projections.Partitioned;
using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class PartitionedProjectionObserver<TProjection, TEvent> : IObserver<SubscriptionMessage>
        where TProjection : PartitionedProjection, new()
        where TEvent : Event
    {
        private readonly IStreamFilter _filter;
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionObserver(IServiceProvider serviceProvider, IStreamFilter filter)
        {
            _filter = filter;
            _serviceProvider = serviceProvider;
        }

        public void OnCompleted()
        {

        }

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
                var handler = scope.ServiceProvider.GetService<IPartitionedProjectionEventHandler<TProjection, TEvent>>();
                if (handler == null)
                    return;

                var repository = scope.ServiceProvider.GetRequiredService<IPartitionedProjectionStateRepository<TProjection>>();

                var readModelQuery = repository.GetProjectionState(typedEvent.AggregateId);
                readModelQuery.Wait();

                var readModel = readModelQuery.Result;
                if (readModel == null)
                    readModel = new TProjection();

                var projectionTask = handler.ProjectEvent(readModel, typedEvent);
                projectionTask.Wait();
                projectionTask.Result.LastEventId = value.Event.EventId;

                var updateTask = repository.SetProjectionState(typedEvent.AggregateId, projectionTask.Result);
                updateTask.Wait();
            }
        }
    }
}
