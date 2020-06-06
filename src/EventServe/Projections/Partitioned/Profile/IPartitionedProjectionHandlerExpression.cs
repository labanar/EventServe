using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionHandlerExpression<T>
        where T: PartitionedProjection
    {
        IPartitionedProjectionHandlerExpression<T> HandleEvent<TEvent>() where TEvent : Event;
    }
}
