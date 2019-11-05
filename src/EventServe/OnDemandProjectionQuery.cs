using System.Threading.Tasks;
using EventServe.Services;

namespace EventServe {
    public class OnDemandProjectionQuery<T> : IProjectionQuery<T>
        where T : IProjection, new() {
            private readonly IEventStreamReader _streamReader;
            private readonly IProjectionEventHandler<T> _handler;

            public OnDemandProjectionQuery(IProjectionEventHandler<T> handler, IEventStreamReader streamReader) {
                _streamReader = streamReader;
                _handler = handler;
            }

            public async Task<T> Execute(string streamId) {
                var projection = new T();

                var events = await _streamReader.ReadAllEventsFromStream(streamId);
                foreach (var @event in events) {
                    projection = await _handler.HandleEvent(projection, @event);
                    projection.IncrementVersion();
                }

                return projection;
            }
        }
}