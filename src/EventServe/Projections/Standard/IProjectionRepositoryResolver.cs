using System.Threading.Tasks;

namespace EventServe.Projections.Standard
{
    public interface IProjectionRepositoryResolver
    {
        Task<IProjectionStateRepository> Resolve();
    }
}
