using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionProfileExpression<T>
        where T: PartitionedProjection
    {
        IPartitionedProjectionHandlerExpression<T> ProjectFromAggregateCategory<TAggregate>() where TAggregate : AggregateRoot;
    }
}
