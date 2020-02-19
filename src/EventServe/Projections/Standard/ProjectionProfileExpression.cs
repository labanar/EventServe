using System;
using System.Collections.Generic;

namespace EventServe.Projections
{
    public class ProjectionProfileExpression : 
        IProjectionProfileExpression, 
        IProjectionHandlerExpression
    {

        private readonly ProjectionFilterBuilder _filterBuilder = new ProjectionFilterBuilder();
        private readonly HashSet<Type> _eventTypes;

        public ProjectionProfileExpression(ProjectionFilterBuilder filterBuilder, HashSet<Type> eventTypes)
        {
            _filterBuilder = filterBuilder;
            _eventTypes = eventTypes;
        }

        public IProjectionHandlerExpression HandleEvent<T>() where T : Event
        {
            _filterBuilder.HandleEvent<T>();
            _eventTypes.Add(typeof(T));
            return this;
        }

        public IProjectionHandlerExpression ProjectFromAggregate<T>(Guid id) where T : AggregateRoot
        {
            _filterBuilder.ProjectFromAggregate<T>(id);
            return this;
        }

        public IProjectionHandlerExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot
        {
            _filterBuilder.ProjectFromAggregateCategory<T>();
            return this;
        }
    }
}
