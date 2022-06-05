using System;
using System.Text.Json.Serialization;

namespace EventServe
{
    public abstract class Event
    {
        public Guid EventId { get; set; }

        public DateTime EventDate { get; set; }

        public Guid AggregateId { get; set; }

        internal Event(Guid aggregateId, bool allowDefaultGuid = false)
        {
            if (aggregateId == null || (!allowDefaultGuid && aggregateId == default(Guid)))
                throw new AggregateException("Null aggregateId provided.");

            AggregateId = aggregateId;
            EventId = Guid.NewGuid();
            EventDate = DateTime.UtcNow;
        }

        public Event(Guid aggregateId)
        {
            if (aggregateId == null || aggregateId == default(Guid))
                throw new AggregateException("Null aggregateId provided.");

            AggregateId = aggregateId;
            EventId = Guid.NewGuid();
            EventDate = DateTime.UtcNow;
        }

        protected Event() { }
    }

    public class EventMetaData
    {
        private EventMetaData() { }

        public EventMetaData(Event @event)
        {
            AssemblyQualifiedName = @event.GetType().AssemblyQualifiedName;
        }

        [JsonConstructorAttribute]
        public EventMetaData(string assemblyQualifiedName, string issuedBy)
        {
            AssemblyQualifiedName = assemblyQualifiedName;
            IssuedBy = issuedBy;
        }

        public string AssemblyQualifiedName { get; private set; }
        public string IssuedBy { get; private set; }
    }
}
