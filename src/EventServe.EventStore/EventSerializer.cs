using EventServe.EventStore.Interfaces;
using EventStore.ClientAPI;
using System;
using System.Text;
using System.Text.Json;

namespace EventServe.EventStore
{
    public class EventSerializer : IEventSerializer
    {
        public Event DeseralizeEvent(ResolvedEvent resolvedEvent)
        {
            return DeserializeEvent(resolvedEvent);
        }

        public EventData SerializeEvent(Event @event)
        {
            return SeralizeEvent(@event);
        }

        private Event DeserializeEvent(ResolvedEvent resolvedEvent)
        {
            var metaData = JsonSerializer.Deserialize<EventMetaData>(resolvedEvent.Event.Metadata);
            var eventType = Type.GetType(metaData.AssemblyQualifiedName);

            var @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data, eventType) as Event;
            return @event;
        }

        private EventData SeralizeEvent<T>(T @event) where T :Event
        {
            var type = @event.GetType();
            var typeName = type.FullName;

            var serializedEvent = JsonSerializer.SerializeToUtf8Bytes(@event, type);

            var metaData = new EventMetaData(@event);
            var serializedMetaData = JsonSerializer.SerializeToUtf8Bytes(metaData, typeof(EventMetaData));
            return new EventData(@event.EventId, typeName, true, serializedEvent, serializedMetaData);
        }
    }
}
