using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventStreamWriter
    {
        Task AppendEventToStream<T>(string stream, T @event) where T : Event;
        Task AppendEventToStream<T>(string stream, T @event, long? expectedVersion) where T : Event;
        Task AppendEventsToStream<T>(string stream, List<T> events) where T : Event;
        Task AppendEventsToStream<T>(string stream, List<T> events, long? expectedVersion) where T : Event;
    }
}
