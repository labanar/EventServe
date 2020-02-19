using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IProjectionHandlerResolver
    {
        Task<IProjectionEventHandler<TProjection, TEvent>> Resolve<TProjection, TEvent>()
            where TProjection : Projection
            where TEvent : Event;
    }

    public class ProjectionHandlerResolver : IProjectionHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ProjectionHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        Task<IProjectionEventHandler<TProjection, TEvent>> IProjectionHandlerResolver.Resolve<TProjection, TEvent>()
        {
            var handler = (IProjectionEventHandler<TProjection, TEvent>)_serviceProvider
                            .GetService(typeof(IProjectionEventHandler<TProjection, TEvent>));

            return Task.FromResult(handler);
        }
    }
}
