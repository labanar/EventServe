using System;
using System.Collections.Generic;

namespace EventServe.Projections.Partitioned
{
    public class PartitionedProjectionProfile
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

        public IPartitionedProjectionProfileExpression<TProjection> CreateProfile<TProjection>()
            where TProjection: PartitionedProjection
        {
            //idk how I feel about this
            if (_projectionType != default)
                throw new Exception("You cannot define more than one projection profile per class.");

            _projectionType = typeof(TProjection);
            return new PartitionedProjectionProfileExpression<TProjection>(_projectionFilterBuilder, _eventTypes);
        }
    }
}
