using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionHandlerExpression
    {
        IPartitionedProjectionHandlerExpression HandleEvent<T>() where T : Event;
    }
}
