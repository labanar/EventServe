using System.Threading.Tasks;

namespace EventServe.Projections.Partitioned
{
    public interface IPartitionedProjectionRepositoryResolver
    {
        Task<IPartitionedProjectionStateRepository> Resolve();
    }
}
