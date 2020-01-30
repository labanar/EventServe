using SqlStreamStore.Streams;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore
{
    public interface IEventSerializer
    {
        Task<Event> DeseralizeEvent(StreamMessage streamMessage);

        Task<NewStreamMessage> SerializeEvent(Event @event);
    }

    public class SqlStreamStoreEventSerializer : IEventSerializer
    {
        public async Task<Event> DeseralizeEvent(StreamMessage streamMessage)
        {
            var metaData = JsonSerializer.Deserialize<EventMetaData>(streamMessage.JsonMetadata);
            var eventType = Type.GetType(metaData.AssemblyQualifiedName);
            var @event = JsonSerializer.Deserialize(await streamMessage.GetJsonData(), eventType) as Event;
            return @event;
        }

        public Task<NewStreamMessage> SerializeEvent(Event @event)
        {
            var type = @event.GetType();
            var typeName = type.FullName;

            var serializedEvent =  JsonSerializer.Serialize(@event, type);

            var metaData = new EventMetaData(@event);
            var serializedMetaData = JsonSerializer.Serialize(metaData, typeof(EventMetaData));

            return Task.FromResult(new NewStreamMessage(@event.EventId, typeName, serializedEvent, serializedMetaData));
        }
    }
}
