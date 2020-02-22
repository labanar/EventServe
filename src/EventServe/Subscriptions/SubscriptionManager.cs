using EventServe.Subscriptions.Domain;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionManager
    {
        IAsyncEnumerable<string> GetSubscriptions();
        Task Add(Guid subscriptionId, ITransientStreamSubscriptionConnection subscription, TransientStreamSubscriptionConnectionSettings connectionSettings);
        Task Add(Guid subscriptionId, IPersistentStreamSubscriptionConnection subscription, PersistentStreamSubscriptionConnectionSettings connectionSettings);
        Task Connect(Guid subscriptionId);
        Task Disconnect(Guid subscriptionId);
        Task Reset(Guid subscriptionId);
    }

    public class SubscriptionManager : ISubscriptionManager
    {
        private Dictionary<Guid, (ITransientStreamSubscriptionConnection Connection, TransientStreamSubscriptionConnectionSettings ConnectionSettings)> _transientSubscriptions =
            new Dictionary<Guid, (ITransientStreamSubscriptionConnection Connection, TransientStreamSubscriptionConnectionSettings ConnectionSettings)>();

        private Dictionary<Guid, (IPersistentStreamSubscriptionConnection Connection, PersistentStreamSubscriptionConnectionSettings ConnectionSettings)> _persistentSubscriptions =
            new Dictionary<Guid, (IPersistentStreamSubscriptionConnection Connection, PersistentStreamSubscriptionConnectionSettings ConnectionSettings)>();
        
        
        public async IAsyncEnumerable<string> GetSubscriptions()
        {
            //Subscription Name
            //Subscription Id
            //Status
            //Uptime
            //Position
            //Position Relative to End

            yield break;
        }

        public Task Add(Guid subscriptionId, ITransientStreamSubscriptionConnection subscription, TransientStreamSubscriptionConnectionSettings connectionSettings)
        {
            _transientSubscriptions[subscriptionId] = (subscription, connectionSettings);
            return Task.CompletedTask;
        }
        public Task Add(Guid subscriptionId, IPersistentStreamSubscriptionConnection subscription, PersistentStreamSubscriptionConnectionSettings connectionSettings)
        {
            _persistentSubscriptions[subscriptionId] = (subscription, connectionSettings);
            return Task.CompletedTask;
        }

        public async Task Connect(Guid subscriptionId)
        {
            if (_persistentSubscriptions.TryGetValue(subscriptionId, out var pSub))
                await pSub.Connection.Connect(pSub.ConnectionSettings);
            else if (_transientSubscriptions.TryGetValue(subscriptionId, out var tSub))
                await tSub.Connection.Connect(tSub.ConnectionSettings);
        }

        public async Task Disconnect(Guid subscriptionId)
        {
            if (_persistentSubscriptions.TryGetValue(subscriptionId, out var pSub))
                await pSub.Connection.Disconnect();
            else if (_transientSubscriptions.TryGetValue(subscriptionId, out var tSub))
                await tSub.Connection.Disconnect();
        }

 
        public async Task Reset(Guid subscriptionId)
        {
            if (_persistentSubscriptions.TryGetValue(subscriptionId, out var pSub))
                await pSub.Connection.Reset();
        }
    }


    public class SubscriptionManagerProfile : TransientSubscriptionProfile
    {
        public SubscriptionManagerProfile()
        {
            CreateProfile()
                .StartFromEnd()
                .SubscribeToAggregate<SubscriptionManagerRoot>(Guid.Empty)
                .HandleEvent<SubscriptionStartedEvent>()
                .HandleEvent<SubscriptionStoppedEvent>();
        }


        public class Handler :
            ISubscriptionEventHandler<SubscriptionManagerProfile, SubscriptionStartedEvent>,
            ISubscriptionEventHandler<SubscriptionManagerProfile, SubscriptionStoppedEvent>
{
            private readonly ISubscriptionManager _manager;

            public Handler(ISubscriptionManager manager)
            {
                _manager = manager;
            }

            public async Task HandleEvent(SubscriptionStartedEvent @event)
            {
                await _manager.Connect(@event.SubscriptionId);
            }

            public async Task HandleEvent(SubscriptionStoppedEvent @event)
            {
                await _manager.Disconnect(@event.SubscriptionId);
            }
        }
    }
}
