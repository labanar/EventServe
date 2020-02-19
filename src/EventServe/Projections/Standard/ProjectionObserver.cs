using EventServe.Projections.Standard;
using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public class ProjectionObserver<TProjection, TEvent> : IObserver<Event>
        where TProjection : Projection, new()
        where TEvent : Event
    {
        private readonly IProjectionRepositoryResolver _repoResolver;
        private readonly IProjectionHandlerResolver _handlerResolver;

        public ProjectionObserver(
            IProjectionHandlerResolver handlerResolver,
            IProjectionRepositoryResolver repoResolver)
        {
            _repoResolver = repoResolver;
            _handlerResolver = handlerResolver;
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

            var worker = Task.Factory
                .StartNew(async () =>
                {
                    var handler = await _handlerResolver.Resolve<TProjection, TEvent>();
                    if (handler == null)
                        return;

                    var repository = await _repoResolver.Resolve();
                    if (repository == null)
                        return;

                    var readModel = await repository.GetProjectionState<TProjection>();
                    if (readModel == null)
                        readModel = new TProjection();

                    await handler.ProjectEvent(readModel, typedEvent);
                    await repository.SetProjectionState(readModel);
                });

            try
            {
                worker.Wait();
            }
            catch(AggregateException ae)
            {
                //Check if the task threw any exceptions that we're concerned with
                foreach(var e in ae.InnerExceptions)
                {

                }
            }
        }
    }
}
