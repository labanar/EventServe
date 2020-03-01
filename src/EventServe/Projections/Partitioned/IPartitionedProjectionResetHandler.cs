using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionResetHandler<TProjection>
        where TProjection : PartitionedProjection
    {
        Task HandleReset();
    }
}
