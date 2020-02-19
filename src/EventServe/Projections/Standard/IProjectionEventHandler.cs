using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IProjectionEventHandler<TProjection, TEvent>
        where TProjection: Projection
        where TEvent: Event
    {
        Task<TProjection> ProjectEvent(TProjection prevState, TEvent @event);
    }
}
