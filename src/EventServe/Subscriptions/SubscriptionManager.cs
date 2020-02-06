using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionManager
    {
        Task AddSubscription(SubscriptionBase subscription);
        Task StartSubscription(Guid subscriptionId);
        Task StopSubscription(Guid subscriptionId);
    }

    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IServiceProvider _serivceProvider;
        private Dictionary<Guid, ITransientStreamSubscription> _transientSubscriptions = new Dictionary<Guid, ITransientStreamSubscription>();
        private Dictionary<Guid, IPersistentStreamSubscription> _persistentSubscriptions = new Dictionary<Guid, IPersistentStreamSubscription>();
        private Dictionary<Guid, SubscriptionBase> _subscriptions = new Dictionary<Guid, SubscriptionBase>();

        //TODO - anyway to avoid the service locator pattern here?
        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            _serivceProvider = serviceProvider;
        }


        public Task AddSubscription(SubscriptionBase subscription)
        {
            if(subscription.Type == Domain.Enums.SubscriptionType.Persistent)
            {
                var sub = _serivceProvider.GetService(typeof(IPersistentStreamSubscription)) as IPersistentStreamSubscription;
                _persistentSubscriptions[subscription.SubscriptionId] = sub;
            }
            else
            {
                var sub = _serivceProvider.GetService(typeof(ITransientStreamSubscription)) as ITransientStreamSubscription;
                _transientSubscriptions[subscription.SubscriptionId] = sub;
            }

            _subscriptions[subscription.SubscriptionId] = subscription;
            return Task.CompletedTask;
        }

        public Task AddTransentSubscription(Guid subscriptionId)
        {
            var subscription = _serivceProvider.GetService(typeof(ITransientStreamSubscription)) as ITransientStreamSubscription;
            _transientSubscriptions[subscriptionId] = subscription;
            return Task.CompletedTask;
        }

        public async Task StartSubscription(Guid subscriptionId)
        {
            var subscription = _subscriptions[subscriptionId];
            if (subscription == null)
                return;

            //if (subscription.Type == Domain.Enums.SubscriptionType.Persistent)
            //    await _persistentSubscriptions[subscriptionId].Start(subscriptionId, subscription.StreamId);
            //else
            //    await _transientSubscriptions[subscriptionId].Start(subscriptionId, subscription.StreamId);
        }

        public async Task StopSubscription(Guid subscriptionId)
        {
            var subscription = _subscriptions[subscriptionId];
            if (subscription == null)
                return;

            if (subscription.Type == Domain.Enums.SubscriptionType.Persistent)
                await _persistentSubscriptions[subscriptionId].Stop();
            else
                await _transientSubscriptions[subscriptionId].Stop();
        }
    }  
}
