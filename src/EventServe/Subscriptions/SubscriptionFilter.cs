using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventServe.Subscriptions
{
    public class SubscriptionFilterBuilder
    {
        private StreamId _streamId;
        private Type _aggregateType;
        private readonly HashSet<string> _streamExpressions = new HashSet<string>();

        public SubscriptionFilterBuilder() { }

        public SubscriptionFilterBuilder SubscribeToAggregate<T>(Guid id)
            where T: AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot subscribe to more than one aggregate type per subscription.");

            _aggregateType = typeof(T);
            if (_streamId == null)
                _streamId = new StreamId($"{typeof(T).Name.ToUpper()}-{id}");

            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{id}$");
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToAggregateCategory<T>()
         where T : AggregateRoot
        {
            if (_aggregateType != null && _aggregateType != typeof(T))
                throw new ArgumentException("You cannot subscribe to more than one aggregate type per subscription.");

            _aggregateType = typeof(T);
            var guidExpression = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{guidExpression}$");
            return this;
        }
        public SubscriptionFilter Build()
        {
            //TODO - Argument check
            if (_aggregateType != null)
                return new SubscriptionFilter(_aggregateType, _streamExpressions);
            else
                return new SubscriptionFilter(_streamId, _streamExpressions);
        }
    }


    public class SubscriptionFilter
    {
        private readonly StreamId _streamId;
        private readonly HashSet<string> _streamExpressions;
        private readonly Type _aggregateType;

        public StreamId SubscribedStreamId => _streamId;
        public Type AggregateType => _aggregateType;

        public SubscriptionFilter(StreamId streamId, HashSet<string> streamExpressions)
        {
            _streamId = streamId;
            _streamExpressions = streamExpressions;
        }

        public SubscriptionFilter(Type aggregateType, HashSet<string> streamExpressions)
        {
            _aggregateType = aggregateType;
            _streamExpressions = streamExpressions;
        }


        public bool DoesStreamIdPassFilter(string streamId)
        {
            if (_aggregateType != null)
            {
                foreach (var pattern in _streamExpressions)
                {
                    if (Regex.IsMatch(streamId, pattern, RegexOptions.IgnoreCase))
                        return true;
                }
            }

            return _streamId.Id == streamId;
        }
    }
}
