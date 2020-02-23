using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class ProjectionObserver<TProjection, TEvent> : IObserver<SubscriptionMessage>
        where TProjection : Projection, new()
        where TEvent : Event
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStreamFilter _filter;

        public ProjectionObserver(IServiceProvider serviceProvider, IStreamFilter streamFilter)
        {
            _serviceProvider = serviceProvider;
            _filter = streamFilter;
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
                var handler = scope.ServiceProvider.GetService<IProjectionEventHandler<TProjection, TEvent>>();
                if (handler == null)
                    return;

                var repository = scope.ServiceProvider.GetRequiredService<IProjectionStateRepository>();

                var readModelQuery = repository.GetProjectionState<TProjection>();
                readModelQuery.Wait();
                var readModel = readModelQuery.Result;
                if (readModel == null)
                    readModel = new TProjection();

                var projectionTask = handler.ProjectEvent(readModel, typedEvent);
                projectionTask.Wait();

                var updateTask = repository.SetProjectionState(readModel);
                updateTask.Wait();
            }
        }
    }
}
