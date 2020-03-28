using ReflectionMagic;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EventServe
{
    public abstract class AggregateRoot
    {
        public abstract Guid Id { get; }
        public long? Version => _version;

        private long? _version = null;

        private IReadOnlyCollection<Event> DomainEvents => _changes.AsReadOnly();

        private readonly List<Event> _changes = new List<Event>();

        public List<Event> GetUncommitedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            if (_version.HasValue)
                _version += _changes.Count;
            else
                _version = _changes.Count - 1;

            _changes.Clear();
        }

        public void LoadFromHistory(Event @event)
        {
            if (@event == null)
                return;

            ApplyChange(@event, false);
            if (_version.HasValue)
                _version += 1;
            else
                _version = 0;
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
