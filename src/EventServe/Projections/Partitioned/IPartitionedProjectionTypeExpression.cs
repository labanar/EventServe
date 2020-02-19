using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionTypeExpression
    {
        IPartitionedProjectionHandlerExpression OnTo<T>() where T : PartitionedProjection;
    }
}
