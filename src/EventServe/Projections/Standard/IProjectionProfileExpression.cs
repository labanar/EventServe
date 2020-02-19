using EventServe.Projections.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections
{
    public interface IProjectionProfileExpression
    {
        IProjectionTypeExpression ProjectFromAggregate<T>(Guid id) where T : AggregateRoot;
        IProjectionTypeExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot;
    }
}
