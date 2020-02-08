using EventServe.Subscriptions.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionManager
    {
        Task AddSubscription(Guid subscriptionId, ITransientStreamSubscription subscription);
        Task AddSubscription(Guid subscriptionId, IPersistentStreamSubscription subscription);
        Task StartSubscription(Guid subscriptionId);
        Task StopSubscription(Guid subscriptionId);
    }

    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IServiceProvider _serivceProvider;
        private Dictionary<Guid, ITransientStreamSubscription> _transientSubscriptions = new Dictionary<Guid, ITransientStreamSubscription>();
        private Dictionary<Guid, IPersistentStreamSubscription> _persistentSubscriptions = new Dictionary<Guid, IPersistentStreamSubscription>();

        //TODO - anyway to avoid the service locator pattern here?
        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            _serivceProvider = serviceProvider;
        }


        public Task AddSubscription(Guid subscriptionId, ITransientStreamSubscription subscription)
        {
            _transientSubscriptions[subscriptionId] = subscription;
            return Task.CompletedTask;
        }
        public Task AddSubscription(Guid subscriptionId, IPersistentStreamSubscription subscription)
        {
            _persistentSubscriptions[subscriptionId] = subscription;
            return Task.CompletedTask;
        }
      
        public async Task StartSubscription(Guid subscriptionId)
        {


            //if (subscription.Type == Domain.Enums.SubscriptionType.Persistent)
            //    await _persistentSubscriptions[subscriptionId].Start(subscriptionId, subscription.StreamId);
            //else
            //    await _transientSubscriptions[subscriptionId].Start(subscriptionId, subscription.StreamId);
        }

        public async Task StopSubscription(Guid subscriptionId)
        {


          
        }
    }
}
