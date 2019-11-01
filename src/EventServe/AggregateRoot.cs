using ReflectionMagic;
using System;
using System.Collections.Generic;

namespace EventServe
{
    public abstract class AggregateRoot
    {
        public abstract Guid Id { get; }
        public int Version { get; internal set; } = -1;

        private IReadOnlyCollection<Event> DomainEvents => _changes.AsReadOnly();

        private readonly List<Event> _changes = new List<Event>();

        public List<Event> GetUncommitedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            Version += _changes.Count;
            _changes.Clear();
        }

        public void LoadFromHistory(IEnumerable<Event> history)
        {
            foreach (var @event in history)
            {
                ApplyChange(@event, false);
                Version++;
            }
        }

        private void ApplyChange(Event @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
                _changes.Add(@event);
        }

        protected void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }
    }
}
