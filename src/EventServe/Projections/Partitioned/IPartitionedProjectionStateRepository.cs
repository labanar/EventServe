using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IPartitionedProjectionStateRepository<T>
        where T: PartitionedProjection
    {
        Task<T> GetProjectionState(Guid partitionId);
        Task SetProjectionState(Guid partitionId, T state);
        Task ResetState();
    }
}
