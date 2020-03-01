using System;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    public class DummyAggregate : AggregateRoot
    {
        public override Guid Id => _id;

        private Guid _id;
        private string _name;
        private string _url;

        private DummyAggregate() { }

        public DummyAggregate(ResetDummyAggregateCommand command)
        {
            ApplyChange(new DummyCreatedEvent(command.Id, command.Name, command.Url));
        }

        public void ResetDummy(ResetDummyAggregateCommand command)
        {
            if (command.Url.Equals(_url, StringComparison.InvariantCultureIgnoreCase))
                ApplyChange(new DummyUrlChangedEvent(_id, command.Url));

            if (command.Name.Equals(_name, StringComparison.InvariantCultureIgnoreCase))
                ApplyChange(new DummyNameChangedEvent(_id, command.Name));
        }

        private void Apply(DummyCreatedEvent @event)
        {
            _id = @event.AggregateId;
            _name = @event.Name;
            _url = @event.Url;
        }

        private void Apply(DummyNameChangedEvent @event)
        {
            _name = @event.Name;
        }

        private void Apply(DummyUrlChangedEvent @event)
        {
            _url = @event.Url;
        }
    }


    public class DummyCreatedEvent : Event
    {
        public DummyCreatedEvent() { }

        public DummyCreatedEvent(Guid aggregateId, string name, string url) : base(aggregateId)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class DummyNameChangedEvent : Event
    {
        public DummyNameChangedEvent() { }

        public DummyNameChangedEvent(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class DummyUrlChangedEvent : Event
    {
        public DummyUrlChangedEvent() { }

        public DummyUrlChangedEvent(Guid aggregateId, string url) : base(aggregateId)
        {
            Url = url;
        }

        public string Url { get; set; }
    }


    public class ResetDummyAggregateCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
