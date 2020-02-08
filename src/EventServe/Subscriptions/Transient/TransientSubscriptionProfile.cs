using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Subscriptions.Transient
{
    public interface ITransientSubscriptionProfile
    {
        SubscriptionFilter Filter { get; }
        HashSet<Type> SubscribedEvents { get; }
        int StartPosition { get; }

        ITransientSubscriptionPositionExpression CreateProfile();
    }

    public abstract class TransientSubscriptionProfile : ITransientSubscriptionProfile, ISubscriptionProfile
    {
        public SubscriptionFilter Filter => _subscriptionFilterBuilder.Build();
        public HashSet<Type> SubscribedEvents => _subscribedEvents;
        public int StartPosition => _position.Position;

        private readonly SubscriptionFilterBuilder _subscriptionFilterBuilder;
        private readonly HashSet<Type> _subscribedEvents;
        private StreamPosition _position = StreamPosition.EndOfStream();

        public TransientSubscriptionProfile()
        {
            _subscriptionFilterBuilder = new SubscriptionFilterBuilder();
            _subscribedEvents = new HashSet<Type>();
        }

        public ITransientSubscriptionPositionExpression CreateProfile()
        {
            var expression = new TransientSubscriptionProfileExpression(_subscriptionFilterBuilder, _subscribedEvents, _position);
            return expression;
        }
    }
}
