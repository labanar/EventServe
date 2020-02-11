using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EventServe.Subscriptions.Transient;

namespace EventServe.Subscriptions
{
    public interface ITransientStreamSubscriptionConnection: IObservable<Event>
    {
        Task Connect(TransientStreamSubscriptionConnectionSettings connectionSettings);
        Task Disconnect();
    }

    public abstract class TransientStreamSubscriptionConnection : ITransientStreamSubscriptionConnection
    {
        private readonly Queue<Task> _dispatchQueue = new Queue<Task>();
        private readonly SemaphoreLocker _locker;
        protected bool _connected = false;
        protected SubscriptionFilter _filter;
        protected int _startPosition = 0;
        protected bool _cancellationRequestedByUser = false;
        private List<IObserver<Event>> _observers = new List<IObserver<Event>>();

        public TransientStreamSubscriptionConnection()
        {
            _locker = new SemaphoreLocker();
        }

        public async Task Connect(TransientStreamSubscriptionConnectionSettings connectionSettings)
        {
            _filter = connectionSettings.Filter;
            _startPosition = connectionSettings.StartPosition;
            await ConnectAsync();
        }
        public async Task Disconnect()
        {
            _cancellationRequestedByUser = true;
            await DisconnectAsync();
        }

        public IDisposable Subscribe(IObserver<Event> observer)
        {
            _observers.Add(observer);
            return default;
        }

        protected abstract Task ConnectAsync();
        protected abstract Task DisconnectAsync();

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
            foreach(var observer in _observers)
                observer.OnNext(@event);
        }
    }
}
