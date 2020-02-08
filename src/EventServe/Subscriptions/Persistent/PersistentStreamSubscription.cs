using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EventServe.Subscriptions
{
    public interface IPersistentStreamSubscription : IObservable<Event>
    {
        Task Start(string subscriptionName, SubscriptionFilter filter);
        Task Stop();
    }

    public abstract class PersistentStreamSubscription : IPersistentStreamSubscription
    {
        private readonly Queue<Task> _dispatchQueue;
        private readonly SemaphoreLocker _locker;
        protected bool _connected = false;
        protected bool _cancellationRequestedByUser = false;
        private List<IObserver<Event>> _observers = new List<IObserver<Event>>();
        protected string _subscriptionName;
        protected SubscriptionFilter _filter;

        public PersistentStreamSubscription()
        {
            _dispatchQueue = new Queue<Task>();
            _locker = new SemaphoreLocker();
        }

        public async Task Start(string subscriptionName, SubscriptionFilter filter)
        {
            _subscriptionName = subscriptionName;
            _filter = filter;
            await ConnectAsync();
        }   
        public async Task Stop()
        {
            _cancellationRequestedByUser = true;
            await DisconnectAsync();
        }

        protected abstract Task ConnectAsync();
        protected abstract Task DisconnectAsync();
        protected abstract Task AcknowledgeEvent<T>(T @event) where T : Event;
        protected async Task RaiseEvent<T>(T @event, string sourceStreamId) where T : Event
        {
            //Check if this event passes through the filter
            if (_filter != null && !_filter.DoesStreamIdPassFilter(sourceStreamId))
                await AcknowledgeEvent(@event);

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

                await AcknowledgeEvent(@event);
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
