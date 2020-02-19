using System;
using System.Collections.Generic;

namespace EventServe.Projections
{
    public abstract class ProjectionProfile
    {
        public ProjectionFilter Filter => _projectionFilterBuilder.Build();
        public HashSet<Type> SubscribedEvents => _eventTypes;

        private readonly ProjectionFilterBuilder _projectionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;

        public ProjectionProfile()
        {
            _projectionFilterBuilder = new ProjectionFilterBuilder();
            _eventTypes = new HashSet<Type>();
        }

        public IProjectionProfileExpression CreateProfile()
        {
            var expression = new ProjectionProfileExpression(_projectionFilterBuilder, _eventTypes);
            return expression;
        }
    }
}
