using EventServe.Projections.Standard;
using System;
using System.Collections.Generic;

namespace EventServe.Projections
{
    public abstract class ProjectionProfile :
        IProjectionProfileExpression,
        IProjectionHandlerExpression,
        IProjectionTypeExpression
    {
        public ProjectionFilter Filter => _projectionFilterBuilder.Build();
        public HashSet<Type> SubscribedEvents => _eventTypes;
        public Type ProjectionType => _projectionType;

        private readonly ProjectionFilterBuilder _projectionFilterBuilder;
        private readonly HashSet<Type> _eventTypes;
        private Type _projectionType;

        public ProjectionProfile()
        {
            _projectionFilterBuilder = new ProjectionFilterBuilder();
            _eventTypes = new HashSet<Type>();
        }

        public IProjectionProfileExpression CreateProfile()
        {
            return this;
        }


        public IProjectionHandlerExpression HandleEvent<T>() where T : Event
        {
            _projectionFilterBuilder.HandleEvent<T>();
            _eventTypes.Add(typeof(T));
            return this;
        }

        public IProjectionTypeExpression ProjectFromAggregate<T>(Guid id) where T : AggregateRoot
        {
            _projectionFilterBuilder.ProjectFromAggregate<T>(id);
            return this;
        }

        public IProjectionTypeExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot
        {
            _projectionFilterBuilder.ProjectFromAggregateCategory<T>();
            return this;
        }

        public IProjectionHandlerExpression OnTo<T>() where T : Projection
        {
            _projectionType = typeof(T);
            return this;
        }
    }
}
