using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionQuery<T> where T: PartitionedProjection
    {
        Task<T> Execute(Guid partitionId);
    }

    public class PartitionedProjectionQuery<T> : IPartitionedProjectionQuery<T>
        where T : PartitionedProjection
    {
        public Task<T> Execute(Guid partitionId)
        {
            throw new NotImplementedException();
        }
    }
}
