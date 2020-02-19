using System;
using System.Collections.Generic;

namespace EventServe.Projections
{
    public class ProjectionFilterBuilder
    {
        private StreamId _streamId;
        private Type _aggregateType;
        private readonly HashSet<string> _streamExpressions = new HashSet<string>();
        private readonly HashSet<Type> _eventTypes = new HashSet<Type>();

        public ProjectionFilterBuilder() { }

        public ProjectionFilterBuilder ProjectFromAggregate<T>(Guid id)
            where T : AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot project from more than one aggregate type.");

            _aggregateType = typeof(T);
            if (_streamId == null)
                _streamId = new StreamId($"{typeof(T).Name.ToUpper()}-{id}");

            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{id}$");
            return this;
        }

        public ProjectionFilterBuilder ProjectFromAggregateCategory<T>()
         where T : AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot project from more than one aggregate type.");

            _streamId = null;
            _aggregateType = typeof(T);
            var guidExpression = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{guidExpression}$");
            return this;
        }


        public ProjectionFilterBuilder HandleEvent<T>()
           where T : Event
        {
            _eventTypes.Add(typeof(T));
            return this;
        }

        public ProjectionFilter Build()
        {
            //TODO - Argument check
            if (_streamId != null)
                return new ProjectionFilter(_streamId, _streamExpressions, _eventTypes);
            
            return new ProjectionFilter(_aggregateType, _streamExpressions, _eventTypes);
        }
    }
}
