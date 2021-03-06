﻿using EventServe.EventStore.Interfaces;
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
            var metaDataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
            var eventDataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Data);

            var metaData = JsonSerializer.Deserialize<EventMetaData>(metaDataJson);
            var eventType = Type.GetType(metaData.AssemblyQualifiedName);

            var @event = JsonSerializer.Deserialize(eventDataJson, eventType) as Event;
            return @event;
        }

        private EventData SeralizeEvent<T>(T @event) where T :Event
        {
            var type = @event.GetType();
            var typeName = type.FullName;

            var serializedEvent = JsonSerializer.Serialize(@event, type);
            var dataBytes = Encoding.UTF8.GetBytes(serializedEvent);

            var metaData = new EventMetaData(@event);
            var serializedMetaData = JsonSerializer.Serialize(metaData, typeof(EventMetaData));
            var metaDataBytes = Encoding.UTF8.GetBytes(serializedMetaData);

            return new EventData(@event.EventId, typeName, true, dataBytes, metaDataBytes);
        }
    }
}
