using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Enums;

namespace EventServe.Subscriptions
{
    public interface IPersistentStreamSubscriptionConnection : IObservable<PersistentSubscriptionResetEvent>, IObservable<SubscriptionMessage>
    {
        SubscriptionConnectionStatus Status { get; }
        DateTime? StartDate { get; }
        long? Position { get; }
        public string Name { get; }

        Task Connect(PersistentStreamSubscriptionConnectionSettings settings);
        Task Disconnect();
        Task Reset();
    }

    public abstract class PersistentStreamSubscriptionConnection : IPersistentStreamSubscriptionConnection
    {
        private readonly Queue<Task> _dispatchQueue;
        private readonly SemaphoreLocker _locker;
        private List<IObserver<SubscriptionMessage>> _messageObservers = new List<IObserver<SubscriptionMessage>>();
        private IObserver<PersistentSubscriptionResetEvent> _resetObserver;

        protected bool _connected = false;
        protected DateTime? _startDate;
        protected bool _cancellationRequestedByUser = false;
        protected Guid _subscriptionId;
        protected string _subscriptionName;
        protected StreamId _streamId;
        protected string _aggregateType;
        protected long? _position;
        protected SubscriptionConnectionStatus _status = SubscriptionConnectionStatus.Idle;

        public PersistentStreamSubscriptionConnection()
        {
            _dispatchQueue = new Queue<Task>();
            _locker = new SemaphoreLocker();
        }

        public SubscriptionConnectionStatus Status => _status;
        public long? Position => _position;
        public DateTime? StartDate => _startDate;
        public string Name => _subscriptionName;

        public async Task Connect(PersistentStreamSubscriptionConnectionSettings settings)
        {
            _subscriptionId = settings.SubscriptionId;
            _subscriptionName = settings.SubscriptionName;
            _streamId = settings.StreamId;
            _aggregateType = settings.AggregateType;
            await ConnectAsync();
            _startDate = DateTime.UtcNow;
            _status = SubscriptionConnectionStatus.Connected;
        }
        public async Task Disconnect()
        {
            _cancellationRequestedByUser = true;
            await DisconnectAsync();
            _startDate = null;
        }
        public async Task Reset()
        {
            await DisconnectAsync();
            await ResetAsync();
            _resetObserver.OnNext(new PersistentSubscriptionResetEvent());
            _position = null;
            await ConnectAsync();
        }

        protected abstract Task ConnectAsync();
        protected abstract Task DisconnectAsync();
        protected abstract Task ResetAsync();
        protected abstract Task AcknowledgeEvent(Guid eventId);

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
                foreach (var observer in _messageObservers)
                    observer.OnNext(message);

                await AcknowledgeEvent(message.EventId);
            }
            catch
            {
                throw;
            }
        }

        public IDisposable Subscribe(IObserver<PersistentSubscriptionResetEvent> observer)
        {
            _resetObserver = observer;
            return default;
        }
        public IDisposable Subscribe(IObserver<SubscriptionMessage> observer)
        {
            _messageObservers.Add(observer);
            return default;
        }
    }
}
