using System.Collections.Generic;
using System.Threading.Tasks;
using EventServe.Subscriptions.Notifications;
using MediatR;
using System;
using System.Linq;

namespace EventServe.Subscriptions
{
    public interface IPersistentStreamSubscription
    {
        Task StartAsync(Guid subscriptionId, string streamId);
    }

    public abstract class PersistentStreamSubscription : IPersistentStreamSubscription
    {
        private readonly Queue<Event> _eventQueue;
        private readonly IMediator _mediator;
        private readonly SemaphoreLocker _locker;
        protected  Guid _id;
        protected bool _connected = false;

        public PersistentStreamSubscription(IMediator mediator)
        {
            _eventQueue = new Queue<Event>();
            _mediator = mediator;
            _locker = new SemaphoreLocker();
        }

        public async Task StartAsync(Guid subscriptionId, string streamId)
        {
            _id = subscriptionId;
            await ConnectAsync(streamId);
        }
        protected abstract Task ConnectAsync(string streamId);
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
