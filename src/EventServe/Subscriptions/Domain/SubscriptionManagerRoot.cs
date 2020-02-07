using EventServe.Subscriptions.Domain.Enums;
using System;
using System.Collections.Generic;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionManagerRoot : AggregateRoot
    {
        public override Guid Id => Guid.Empty;
        private Dictionary<Guid, PersistentSubscription> _subscriptions = new Dictionary<Guid, PersistentSubscription>();
        public Dictionary<Guid, PersistentSubscription> Subscriptions => _subscriptions;

        public Guid CreateTransientSubscription(string name, string streamId)
        {
            var subscriptionId = Guid.NewGuid();
            ApplyChange(new SubscriptionCreatedEvent(Guid.NewGuid(), name, streamId, SubscriptionType.Transient));
            return subscriptionId;
        }

        public Guid CreatePersistentSubscription(string name, string streamId)
        {
            var subscriptionId = Guid.NewGuid();
            ApplyChange(new SubscriptionCreatedEvent(Guid.NewGuid(), name, streamId, SubscriptionType.Persistent));
            return subscriptionId;
        }

        public void StopSubscription(Guid id, string reason, Exception exception = null)
        {
            if(_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == false)
                    return;

                ApplyChange(new SubscriptionStoppedEvent(Guid.NewGuid(),reason, exception));
            }
        }

        public void StartSubscription(Guid id)
        {
            if (_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == true)
                    return;

                ApplyChange(new SubscriptionStartedEvent(Guid.NewGuid()));
            }
        }

        public void DeleteSubscription(Guid id)
        {
            if (_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == true)
                    return;

                ApplyChange(new SubscriptionDeletedEvent(Guid.NewGuid()));
            }
        }


        private void Apply(SubscriptionCreatedEvent @event)
        {
            _subscriptions[@event.SubscriptionId] = new PersistentSubscription
            {
                Id = @event.SubscriptionId,
                Name = @event.Name,
                StreamId = @event.StreamId,
                IsConnected = false,
                Type = @event.Type
            };
        }

        private void Apply(SubscriptionStartedEvent @event)
        {
            if (_subscriptions.TryGetValue(@event.SubscriptionId, out var subscription))
                subscription.IsConnected = true;
        }

        private void Apply(SubscriptionStoppedEvent @event)
        {
            if (_subscriptions.TryGetValue(@event.SubscriptionId, out var subscription))
                subscription.IsConnected = false;
        }

        private void Apply(SubscriptionDeletedEvent @event)
        {
            _subscriptions.Remove(@event.SubscriptionId);
        }


        public class PersistentSubscription
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string StreamId { get; set; }
            public bool IsConnected { get; set; }
            public SubscriptionType Type { get; set; }
        }

    }
}
