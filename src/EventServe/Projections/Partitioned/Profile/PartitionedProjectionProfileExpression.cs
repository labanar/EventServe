using System;
using System.Collections.Generic;

namespace EventServe.Projections.Partitioned
{
    public class PartitionedProjectionProfileExpression<T> :
        IPartitionedProjectionProfileExpression<T>,
        IPartitionedProjectionHandlerExpression<T>
        where T: PartitionedProjection
    {

        private readonly ProjectionFilterBuilder _projectionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;

        public PartitionedProjectionProfileExpression(ProjectionFilterBuilder filterBuilder, HashSet<Type> eventTypes)
        {
            _projectionFilterBuilder = filterBuilder;
            _eventTypes = eventTypes;
        }


        public IPartitionedProjectionHandlerExpression<T> ProjectFromAggregateCategory<TAggregate>() 
            where TAggregate : AggregateRoot
        {
            _projectionFilterBuilder.ProjectFromAggregateCategory<TAggregate>();
            return this;
        }

        public IPartitionedProjectionHandlerExpression<T> HandleEvent<TEvent>() 
            where TEvent : Event
        {
            _projectionFilterBuilder.HandleEvent<TEvent>();
            _eventTypes.Add(typeof(TEvent));
            return this;
        }
    }
}
