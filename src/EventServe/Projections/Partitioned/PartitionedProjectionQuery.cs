using EventServe.Services;
using Microsoft.Extensions.DependencyInjection;
using ReflectionMagic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionQuery<T> where T: PartitionedProjection
    {
        Task<T> Execute(Guid partitionId);
    }

    public class PartitionedProjectionQuery<T> : IPartitionedProjectionQuery<T>
        where T : PartitionedProjection, new()
    {
        private readonly IPartitionedProjectionStateRepository<T> _stateRepository;
        private readonly IEventStreamReader _streamReader;
        private readonly PartitionedProjectionProfile<T> _projectionProfile;
        private readonly IServiceProvider _serviceProvider;

        public PartitionedProjectionQuery
            (IPartitionedProjectionStateRepository<T> stateRepository,
            IEventStreamReader streamReader,
            PartitionedProjectionProfile<T> projectionProfile,
            IServiceProvider serviceProvider)
        {
            _stateRepository = stateRepository;
            _streamReader = streamReader;
            _projectionProfile = projectionProfile;
            _serviceProvider = serviceProvider;
        }

        public async Task<T> Execute(Guid partitionId)
        {
            //Fetch the latest state stored in the repository.
            var projection = await _stateRepository.GetProjectionState(partitionId);
            if (projection == null || projection == default)
                projection = new T();

            var streamId = StreamIdBuilder.Create()
                .WithAggregateType(_projectionProfile.Filter.AggregateType)
                .WithAggregateId(partitionId)
                .Build();

            //Read backwards and update read-model if stale
            var eventStack = new Stack<Event>();
            var stream = _streamReader.ReadStreamBackwards(streamId);
            await foreach (var ev in stream)
            {
                if (ev.EventId == projection.LastEventId)
                    break;

                if (_projectionProfile.Filter != null & !_projectionProfile.Filter.DoesEventPassFilter(ev, streamId))
                    continue;

                //add this event to our stack
                eventStack.Push(ev);
            }

            //Pop events off the stack and apply them to the projection
            while(eventStack.Count > 0)
            {
                var @event = eventStack.Pop();
                projection = await ProjectEventOntoModel(projection, @event);
            }

            return projection;
        }

        private async Task<T> ProjectEventOntoModel<T>(T projection, Event @event)
            where T: PartitionedProjection, new()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var handlerType = typeof(IPartitionedProjectionEventHandler<,>).MakeGenericType(typeof(T), @event.GetType());
                var handler = scope.ServiceProvider.GetService(handlerType);

                //TODO - log this? throw? idk?
                if (handler == null)
                    return projection;

                return await (Task<T>)handler.AsDynamic().ProjectEvent(projection, @event);
            }

        }
    }
}
