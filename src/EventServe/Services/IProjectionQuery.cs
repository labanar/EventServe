using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IProjectionQuery<T> where T: IProjection
    {
        Task<T> Execute(string streamId);
    }
}
