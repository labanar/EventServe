using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections
{
    public interface IProjectionProfileExpression
    {
        IProjectionHandlerExpression ProjectFromAggregate<T>(Guid id) where T : AggregateRoot;
        IProjectionHandlerExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot;
    }
}
