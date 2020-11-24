using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionPartitionIdSelector<TProjection, TEvent>
       where TProjection : PartitionedProjection, new()
       where TEvent : Event
    {
        Task<Guid> GetPartitionId(TEvent @event);
    }
}
