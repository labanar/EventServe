using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventServe.Subscriptions
{
    public class SubscriptionFilterBuilder
    {
        private StreamId _streamId;
        private readonly HashSet<string> _streamExpressions = new HashSet<string>();

        public SubscriptionFilterBuilder() { }

        public SubscriptionFilterBuilder SubscribeToAggregate<T>(Guid id)
            where T: AggregateRoot
        {
            if (_streamId == null)
                _streamId = new StreamId($"{typeof(T).Name.ToUpper()}-{id}");
            else
                _streamId = StreamId.All;

            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{id}$");
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToAggregateCategory<T>()
         where T : AggregateRoot
        {
            _streamId = StreamId.All;
            var guidExpression = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{guidExpression}$");
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToStream(string streamId)
        {
            if (_streamId == null)
                _streamId = new StreamId(streamId);
            else
                _streamId = StreamId.All;

            return this;
        }

        public SubscriptionFilter Build()
        {
            return new SubscriptionFilter(_streamId, _streamExpressions);
        }
    }


    public class SubscriptionFilter
    {
        private readonly StreamId _streamId;
        private readonly HashSet<string> _streamExpressions;

        public StreamId SubscribedStreamId => _streamId;


        public SubscriptionFilter(StreamId streamId, HashSet<string> streamExpressions)
        {
            _streamId = streamId;
            _streamExpressions = streamExpressions;
        }

        public bool DoesStreamIdPassFilter(string streamId)
        {
            if (_streamId == StreamId.All)
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
