using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionProfileExpression
    {
        IPartitionedProjectionTypeExpression ProjectFromAggregateCategory<T>() where T : AggregateRoot;
    }
}
