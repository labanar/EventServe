using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EventServe.Subscriptions.Transient;

namespace EventServe.Subscriptions
{
    public interface ITransientStreamSubscriptionConnection: IObservable<SubscriptionMessage>
    {
        Task Connect(TransientStreamSubscriptionConnectionSettings connectionSettings);
        Task Disconnect();
    }

    public abstract class TransientStreamSubscriptionConnection : ITransientStreamSubscriptionConnection
    {
        private readonly Queue<Task> _dispatchQueue = new Queue<Task>();
        private readonly SemaphoreLocker _locker;
        protected bool _connected = false;
        protected StreamId _streamId;
        protected string _aggregateType;
        protected StreamPosition _startPosition = StreamPosition.End;
        protected bool _cancellationRequestedByUser = false;
        private List<IObserver<SubscriptionMessage>> _observers = new List<IObserver<SubscriptionMessage>>();

        public TransientStreamSubscriptionConnection()
        {
            _locker = new SemaphoreLocker();
        }

        public async Task Connect(TransientStreamSubscriptionConnectionSettings connectionSettings)
        {
            _streamId = connectionSettings.StreamId;
            _aggregateType = connectionSettings.AggregateType;
            _startPosition = connectionSettings.StreamPosition;
            await ConnectAsync();
        }
        public async Task Disconnect()
        {
            _cancellationRequestedByUser = true;
            await DisconnectAsync();
        }
        public IDisposable Subscribe(IObserver<SubscriptionMessage> observer)
        {
            _observers.Add(observer);
            return default;
        }

        protected abstract Task ConnectAsync();
        protected abstract Task DisconnectAsync();

        protected async Task RaiseMessage(SubscriptionMessage message)
        {
            //Add event to raising queue
            _dispatchQueue.Enqueue(DispatchMessage(message));

            //Dequeue an event and process
            await _locker.LockAsync(async () =>
            {
                var dequeuedTask = _dispatchQueue.Dequeue();
                await dequeuedTask;
            });
        }
        private async Task DispatchMessage(SubscriptionMessage message)
        {
            try
            {
                foreach (var observer in _observers)
                    observer.OnNext(message);
            }
            catch
            {
                throw;
            }
        }
    }
}
