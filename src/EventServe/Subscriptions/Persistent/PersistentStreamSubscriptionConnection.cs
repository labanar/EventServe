﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EventServe.Subscriptions.Persistent;

namespace EventServe.Subscriptions
{
    public interface IPersistentStreamSubscriptionConnection : IObservable<Event>
    {
        Task Connect(PersistentStreamSubscriptionConnectionSettings settings);
        Task Disconnect();
    }

    public abstract class PersistentStreamSubscriptionConnection : IPersistentStreamSubscriptionConnection
    {
        private readonly Queue<Task> _dispatchQueue;
        private readonly SemaphoreLocker _locker;
        protected bool _connected = false;
        protected bool _cancellationRequestedByUser = false;
        private List<IObserver<Event>> _observers = new List<IObserver<Event>>();
        protected string _subscriptionName;
        protected IStreamFilter _filter;

        public PersistentStreamSubscriptionConnection()
        {
            _dispatchQueue = new Queue<Task>();
            _locker = new SemaphoreLocker();
        }

        public async Task Connect(PersistentStreamSubscriptionConnectionSettings settings)
        {
            _subscriptionName = settings.SubscriptionName;
            _filter = settings.Filter;
            await ConnectAsync();
        }   
        public async Task Disconnect()
        {
            _cancellationRequestedByUser = true;
            await DisconnectAsync();
        }

        protected abstract Task ConnectAsync();
        protected abstract Task DisconnectAsync();
        protected abstract Task AcknowledgeEvent(Guid eventId);
        protected async Task RaiseEvent<T>(T @event) where T : Event
        {
            //Add event to raising queue
            _dispatchQueue.Enqueue(DispatchEvent(@event));

            //Dequeue an event and process
            await _locker.LockAsync(async () =>
            {
                var dequeuedTask = _dispatchQueue.Dequeue();
                await dequeuedTask;
            });
        }
        private async Task DispatchEvent<T>(T @event) where T : Event
        {
            try
            {
                foreach (var observer in _observers)
                    observer.OnNext(@event);

                await AcknowledgeEvent(@event.EventId);
            }
            catch
            {
                throw;
            }
        }
        public IDisposable Subscribe(IObserver<Event> observer)
        {
            _observers.Add(observer);
            return default;
        }
    }
}
