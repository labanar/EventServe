using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class ProjectionObserver<TProjection, TEvent> : IObserver<Event>
        where TProjection : Projection, new()
        where TEvent : Event
    {
        private readonly IServiceProvider _serviceProvider;

        public ProjectionObserver(IServiceProvider serviceProvider)
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
                        var handler = scope.ServiceProvider.GetService<IProjectionEventHandler<TProjection, TEvent>>();
                        if (handler == null)
                            return;

                        var repository = scope.ServiceProvider.GetRequiredService<IProjectionStateRepository>();

                        var readModel = await repository.GetProjectionState<TProjection>();
                        if (readModel == null)
                            readModel = new TProjection();

                        await handler.ProjectEvent(readModel, typedEvent);
                        await repository.SetProjectionState(readModel);
                    }
                });

                worker.Wait();
            }
            catch(AggregateException ae)
            {
                //Check if the task threw any exceptions that we're concerned with
                foreach(var e in ae.InnerExceptions)
                {
                    throw;
                }
            }
        }
    }
}
