using System.Collections.Generic;
using System.Threading.Tasks;
using EventServe.Subscriptions.Notifications;
using MediatR;
using System;

namespace EventServe.Subscriptions
{
    public interface IPersistentSreamSubscription
    {
        Task ConnectAsync(string streamId);
    }

    public class StreamSubscriptionAck
    {
        public Event Event { get; set; }
    }

    public class ARandomHandler : IObservable<StreamSubscriptionAck>
    {
        private List<IObserver<StreamSubscriptionAck>> _subscribers = new List<IObserver<StreamSubscriptionAck>>();
        public IDisposable Subscribe(IObserver<StreamSubscriptionAck> observer)
        {
            _subscribers.Add(observer);
            return default;
        }
    }

    public abstract class PersistentStreamSubscription : IPersistentSreamSubscription
    {
        private readonly Queue<Event> _eventQueue;
        private readonly IMediator _mediator;
        private readonly SemaphoreLocker _locker;
        protected readonly Guid _id;
        protected bool _connected = false;

        public PersistentStreamSubscription(IMediator mediator)
        {
            _eventQueue = new Queue<Event>();
            _mediator = mediator;
            _id = Guid.NewGuid();
            _locker = new SemaphoreLocker();
        }
        public abstract Task ConnectAsync(string streamId);
        protected abstract Task AcknowledgeEvent<T>(T @event) where T : Event;

        protected async Task RaiseEvent<T>(T @event) where T : Event
        {
            //Add event to raising queue
            _eventQueue.Enqueue(@event);

            //Dequeue an event and process
            await _locker.LockAsync(async () =>
            {
                var dequeuedEvent = _eventQueue.Dequeue();
                var notification = new StreamSubscriptionEventNotification(_id, dequeuedEvent, AcknowledgeEvent);
                await _mediator.Publish(notification);
            });
        }
    }
}
