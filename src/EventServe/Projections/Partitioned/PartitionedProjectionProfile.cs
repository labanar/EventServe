using System;
using System.Collections.Generic;

namespace EventServe.Projections.Partitioned
{
    public class PartitionedProjectionProfile :
        IPartitionedProjectionProfileExpression,
        IPartitionedProjectionHandlerExpression,
        IPartitionedProjectionTypeExpression
    {
        public ProjectionFilter Filter => _projectionFilterBuilder.Build();
        public HashSet<Type> SubscribedEvents => _eventTypes;
        public Type ProjectionType => _projectionType;

        private readonly ProjectionFilterBuilder _projectionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;
        private Type _projectionType;
        public PartitionedProjectionProfile()
        {
            _projectionFilterBuilder = new ProjectionFilterBuilder();
            _eventTypes = new HashSet<Type>();
        }

        public IPartitionedProjectionProfileExpression CreateProfile()
        {
            return this;
        }

        public IPartitionedProjectionTypeExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot
        {
            _projectionFilterBuilder.ProjectFromAggregateCategory<T>();
            return this;
        }

        public IPartitionedProjectionHandlerExpression OnTo<T>() where T : PartitionedProjection
        {
            _projectionType = typeof(T);
            return this;
        }

        public IPartitionedProjectionHandlerExpression HandleEvent<T>() where T : Event
        {
            _projectionFilterBuilder.HandleEvent<T>();
            _eventTypes.Add(typeof(T));
            return this;
        }
    }
}
