using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections
{
     public interface IPartitionedProjectionEventHandler<TProjection, TEvent>
        where TProjection : PartitionedProjection, new()
        where TEvent : Event
    {
        Task<TProjection> ProjectEvent(TProjection prevState, TEvent @event);
    }
}
