using EventServe.Projections.Partitioned;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class PartitionedProjectionObserver<TProjection, TEvent> : IObserver<Event>
        where TProjection : PartitionedProjection, new()
        where TEvent : Event
    {
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionObserver(IServiceProvider serviceProvider)
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

                       using(var scope = _serviceProvider.CreateScope())
                       {
                           var handler = scope.ServiceProvider.GetService<IPartitionedProjectionEventHandler<TProjection, TEvent>>();
                           if (handler == null)
                               return;

                           var repository = scope.ServiceProvider.GetRequiredService<IPartitionedProjectionStateRepository>();

                           var readModel = await repository.GetProjectionState<TProjection>(@event.AggregateId);
                           if (readModel == null)
                               readModel = new TProjection();

                           await handler.ProjectEvent(readModel, typedEvent);
                           await repository.SetProjectionState(@event.AggregateId, readModel);
                       }    
                   });

                worker.Wait();
            }
            catch (AggregateException ae)
            {
                //Check if the task threw any exceptions that we're concerned with
                foreach (var e in ae.InnerExceptions)
                {

                }
            }
        }
    }
}
