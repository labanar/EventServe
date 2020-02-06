using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EventServe
{
    public class SubscriptionFilter
    {
        private List<string> _regexFilters = new List<string>();
        public StreamId StreamId => (string.IsNullOrEmpty(_streamId)) ? StreamId.All : new StreamId(_streamId);
        private string _streamId;

        public SubscriptionFilter ListenToStream(string streamId)
        {
            _streamId = streamId;
            return this;
        }

        public SubscriptionFilter ListenToAggregate<T>(Guid aggregateId)
            where T : AggregateRoot
        {
            var streamId = $"{typeof(T).Name.ToUpper()}-{aggregateId}";
            _streamId = streamId;
            return this;
        }

        public SubscriptionFilter ListenToAggregateCategory<T>()
            where T : AggregateRoot
        {
            var guidRegex = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            _regexFilters.Add($"^{typeof(T).Name.ToUpper()}-{guidRegex}$");
            return this;
        }


        public bool DoesStreamIdPassFilter(string streamId)
        {
            if (StreamId != StreamId.All)
                return streamId == StreamId.Id;

            foreach (var pattern in _regexFilters)
            {
                if (Regex.IsMatch(streamId, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }


    public class StreamId : IEquatable<StreamId>
    {
        public string Id { get;}

        public StreamId(string streamId)
        {
            Id = streamId;
        }

        public static StreamId All => new StreamId(Constants.StreamIds.ALL);

        public override bool Equals(object obj)
        {
            return Equals(obj as StreamId);
        }

        public bool Equals([AllowNull] StreamId other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(StreamId left, StreamId right)
        {
            return EqualityComparer<StreamId>.Default.Equals(left, right);
        }

        public static bool operator !=(StreamId left, StreamId right)
        {
            return !(left == right);
        }
    }


    public static class Constants
    {
        public static class StreamIds
        {
            public const string ALL = "__ALL__";
        }
    }
}
