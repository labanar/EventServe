using EventServe.EventStore.Interfaces;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

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
            var metaDataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
            var metaData = JObject.Parse(metaDataJson);

            var eventDataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            var eventType = Type.GetType((string)metaData["AssemblyQualifiedName"]);
            var @event = JsonConvert.DeserializeObject(eventDataJson, eventType) as Event;
            return @event;
        }

        private EventData SeralizeEvent(Event @event)
        {
            var type = @event.GetType();
            var typeName = type.FullName;

            var serializedEvent = JsonConvert.SerializeObject(@event);
            var dataBytes = Encoding.UTF8.GetBytes(serializedEvent);

            var metaData = new EventMetaData(@event);
            var serializedMetaData = JsonConvert.SerializeObject(metaData);
            var metaDataBytes = Encoding.UTF8.GetBytes(serializedMetaData);

            return new EventData(@event.EventId, typeName, true, dataBytes, metaDataBytes);
        }
    }
}
