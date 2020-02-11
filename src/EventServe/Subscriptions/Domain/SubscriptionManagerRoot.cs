using EventServe.Subscriptions.Domain.Enums;
using System;
using System.Collections.Generic;

namespace EventServe.Subscriptions.Domain
{
    public class SubscriptionManagerRoot : AggregateRoot
    {
        public override Guid Id => Guid.Empty;
        private Dictionary<Guid, Subscription> _subscriptions = new Dictionary<Guid, Subscription>();
        public Dictionary<Guid, Subscription> Subscriptions => _subscriptions;

        public Guid CreateTransientSubscription(string name)
        {
            var subscriptionId = Guid.NewGuid();
            ApplyChange(new SubscriptionCreatedEvent(subscriptionId, name, SubscriptionType.Transient));
            return subscriptionId;
        }

        public Guid CreatePersistentSubscription(string name)
        {
            var subscriptionId = Guid.NewGuid();
            ApplyChange(new SubscriptionCreatedEvent(subscriptionId, name, SubscriptionType.Persistent));
            return subscriptionId;
        }

        public void StopSubscription(Guid id, string reason, Exception exception = null)
        {
            if(_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == false)
                    return;

                ApplyChange(new SubscriptionStoppedEvent(id, reason, exception));
            }
        }

        public void StartSubscription(Guid id)
        {
            if (_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == true)
                    return;

                ApplyChange(new SubscriptionStartedEvent(id));
            }
        }

        public void DeleteSubscription(Guid id)
        {
            if (_subscriptions.TryGetValue(id, out var subscription))
            {
                if (subscription.IsConnected == true)
                    return;

                ApplyChange(new SubscriptionDeletedEvent(id));
            }
        }


        private void Apply(SubscriptionCreatedEvent @event)
        {
            _subscriptions[@event.SubscriptionId] = new Subscription
            {
                Id = @event.SubscriptionId,
                Name = @event.Name,
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


        public class Subscription
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsConnected { get; set; }
            public SubscriptionType Type { get; set; }
        }

    }
}
