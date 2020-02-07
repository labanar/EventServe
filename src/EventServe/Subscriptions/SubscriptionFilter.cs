using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventServe.Subscriptions
{
    public class SubscriptionFilterBuilder
    {
        private string _streamId;
        private readonly List<string> _streamExpressions = new List<string>();

        public SubscriptionFilterBuilder() { }

        public SubscriptionFilterBuilder SubscribeToAggregate<T>(Guid id)
            where T: AggregateRoot
        {
            _streamId = $"{typeof(T).Name.ToUpper()}-{id}";
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToAggregateCategory<T>()
         where T : AggregateRoot
        {
            var guidExpression = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _streamExpressions.Add($"^{typeof(T).Name.ToUpper()}-{guidExpression}$");
            return this;
        }

        public SubscriptionFilterBuilder SubscribeToStream(string streamId)
        {
            _streamId = streamId;
            return this;
        }


        public SubscriptionFilter Build()
        {
            if (!string.IsNullOrEmpty(_streamId))
                return new SubscriptionFilter(_streamId);
            else
                return new SubscriptionFilter(_streamExpressions);
        }
    }


    public class SubscriptionFilter
    {
        private readonly string _streamId;
        private readonly StreamId _sId;
        private readonly List<string> _streamExpressions;

        public StreamId SubscribedStreamId => (string.IsNullOrEmpty(_streamId)) ? StreamId.All : _sId;

        public SubscriptionFilter(string streamId)
        {
            _streamId = streamId;
            _sId = new StreamId(streamId);
        }

        public SubscriptionFilter(List<string> streamExpressions)
        {
            _streamExpressions = streamExpressions;
        }

        public bool DoesStreamIdPassFilter(string streamId)
        {
            if (_streamId == StreamId.All.Id)
                return true;

            if (!string.IsNullOrEmpty(_streamId))
                return _streamId == streamId;

            foreach (var pattern in _streamExpressions)
            {
                if (Regex.IsMatch(streamId, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
