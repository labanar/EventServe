using System;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IPartitionedProjectionStateRepository
    {
        Task<T> GetProjectionState<T>(Guid partitionId) where T : PartitionedProjection;
        Task<T> SetProjectionState<T>(Guid partitionId, T state) where T : PartitionedProjection;
    }
}
