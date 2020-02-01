using System;

namespace EventServe
{
    public abstract class Event
    {
        public Guid EventId { get; set; }

        public DateTime EventDate { get; set; }

        public Guid AggregateId { get; set; }

        public Event(Guid aggregateId, bool allowDefaultGuid = false)
        {
            if (aggregateId == null || (!allowDefaultGuid && aggregateId == default(Guid)))
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

        public string AssemblyQualifiedName { get; set; }
        public string IssuedBy { get; set; }
    }
}
