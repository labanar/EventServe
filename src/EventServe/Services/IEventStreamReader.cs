using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventStreamReader
    {
        Task<List<Event>> ReadAllEventsFromStream(string stream);
    }
}
