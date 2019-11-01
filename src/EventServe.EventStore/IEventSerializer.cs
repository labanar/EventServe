using EventStore.ClientAPI;

namespace EventServe.EventStore.Interfaces
{
    public interface IEventSerializer
    {
        Event DeseralizeEvent(ResolvedEvent resolvedEvent);

        EventData SerializeEvent(Event @event);
    }
}
