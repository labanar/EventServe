using System.Collections.Generic;
using System.Threading.Tasks;
using System;


namespace EventServe.Subscriptions
{
    public interface ITransientStreamSubscription: IObservable<Event>
    {
        Task Start(int startPosition, SubscriptionFilter filter);
        Task Stop();
    }

    public abstract class TransientStreamSubscription : ITransientStreamSubscription
    {
        private readonly Queue<Task> _dispatchQueue = new Queue<Task>();
        private readonly SemaphoreLocker _locker;
        protected bool _connected = false;
        protected SubscriptionFilter _filter;
        protected int _startPosition = 0;
        protected bool _cancellationRequestedByUser = false;
        private List<IObserver<Event>> _observers = new List<IObserver<Event>>();

        public TransientStreamSubscription()
        {
            _locker = new SemaphoreLocker();
        }

        public async Task Start(int startPosition, SubscriptionFilter filter)
        {
            _filter = filter;
            _startPosition = startPosition;
            await ConnectAsync();
        }
        public async Task Stop()
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

        protected async Task RaiseEvent<T>(T @event, string sourceStreamId) where T : Event
        {
            //Check if this event passes through the filter
            if (_filter != null && !_filter.DoesStreamIdPassFilter(sourceStreamId))
                return;

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
