using EventServe.Services;
using EventServe.Subscriptions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.Subscriptions
{
    public interface ISubscriptionRootManager
    {
        Task<IEnumerable<SubscriptionBase>> GetSubscriptions();
        Task<SubscriptionBase> CreatePersistentSubscription(string name);
        Task<SubscriptionBase> CreateTransientSubscription(string name);
        Task StopSubscription(Guid subscriptionId);
        Task StartSubscription(Guid subscriptionId);
    }

    public class SubscriptionRootManager : ISubscriptionRootManager
    {
        private readonly IEventRepository<SubscriptionManagerRoot> _repository;

        public SubscriptionRootManager(IEventRepository<SubscriptionManagerRoot> repository)
        {
            _repository = repository;
        }

        public async Task<SubscriptionBase> CreatePersistentSubscription(string name)
        {
            var managerRoot = await _repository.GetById(Guid.Empty);
            if (managerRoot == null) managerRoot = new SubscriptionManagerRoot();
            var subscriptionId = managerRoot.CreatePersistentSubscription(name);
            await _repository.SaveAsync(managerRoot, managerRoot.Version);
            return new SubscriptionBase
            {
                SubscriptionId = subscriptionId,
                Name = name,
                Connected = false,
                Type = Domain.Enums.SubscriptionType.Persistent
            };
        }

        public async Task<SubscriptionBase> CreateTransientSubscription(string name)
        {
            var managerRoot = await _repository.GetById(Guid.Empty);
            if (managerRoot == null) managerRoot = new SubscriptionManagerRoot();
            var subscriptionId = managerRoot.CreateTransientSubscription(name);
            await _repository.SaveAsync(managerRoot, managerRoot.Version);
            return new SubscriptionBase
            {
                SubscriptionId = subscriptionId,
                Name = name,
                Connected = false,
                Type = Domain.Enums.SubscriptionType.Transient
            };
        }

        public async Task<IEnumerable<SubscriptionBase>> GetSubscriptions()
        {
            var managerRoot = await _repository.GetById(Guid.Empty);
            if (managerRoot == null) managerRoot = new SubscriptionManagerRoot();
            return managerRoot.Subscriptions.Values.Select(x => new SubscriptionBase
            {
                SubscriptionId = x.Id,
                Name = x.Name,
                Connected = x.IsConnected
            }).ToList();
        }

        public async Task StartSubscription(Guid subscriptionId)
        {
            var managerRoot = await _repository.GetById(Guid.Empty);
            if (managerRoot == null) managerRoot = new SubscriptionManagerRoot();
            managerRoot.StartSubscription(subscriptionId);
            await _repository.SaveAsync(managerRoot, managerRoot.Version);
        }

        public async Task StopSubscription(Guid subscriptionId)
        {
            var managerRoot = await _repository.GetById(Guid.Empty);
            if (managerRoot == null) managerRoot = new SubscriptionManagerRoot();
            managerRoot.StopSubscription(subscriptionId, "Stopped by user");
            await _repository.SaveAsync(managerRoot, managerRoot.Version);
        }
    }
}
