using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IProjectionStateRepository<T>
        where T : Projection
    {
        Task<T> GetProjectionState();
        Task SetProjectionState(T state);
    }
}
