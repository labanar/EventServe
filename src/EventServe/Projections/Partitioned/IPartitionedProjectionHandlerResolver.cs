using System;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionHandlerResolver
    {
        Task<IPartitionedProjectionEventHandler<TProjection, TEvent>> Resolve<TProjection, TEvent>()
            where TProjection : PartitionedProjection, new()
            where TEvent : Event;
    }

    public class PartitionedProjectionHandlerResolver : IPartitionedProjectionHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IPartitionedProjectionEventHandler<TProjection, TEvent>> Resolve<TProjection, TEvent>()
            where TProjection : PartitionedProjection, new()
            where TEvent : Event
        {
            var handler = (IPartitionedProjectionEventHandler<TProjection, TEvent>)_serviceProvider
                            .GetService(typeof(IPartitionedProjectionEventHandler<TProjection, TEvent>));

            return Task.FromResult(handler);
        }
    }
}
