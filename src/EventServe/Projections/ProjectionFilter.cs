using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventServe.Projections
{
    public class ProjectionFilter: IStreamFilter
    {
        private readonly StreamId _streamId;
        private readonly Type _aggregateType;
        private readonly HashSet<string> _streamExpressions = new HashSet<string>();
        private readonly HashSet<Type> _eventTypes = new HashSet<Type>();
        private readonly HashSet<string> _eventTypeStrings = new HashSet<string>();

        public StreamId SubscribedStreamId => _streamId;
        public Type AggregateType => _aggregateType;

        public ProjectionFilter(StreamId streamId, HashSet<string> streamExpressions, HashSet<Type> eventTypes)
        {
            _streamId = streamId;
            _streamExpressions = streamExpressions;
            _eventTypes = eventTypes;
            _eventTypeStrings = eventTypes.Select(x => x.Name).ToHashSet();
        }

        public ProjectionFilter(Type aggregateType, HashSet<string> streamExpressions, HashSet<Type> eventTypes)
        {
            _aggregateType = aggregateType;
            _streamExpressions = streamExpressions;
            _eventTypes = eventTypes;
            _eventTypeStrings = eventTypes.Select(x => x.Name).ToHashSet();
            _eventTypeStrings.UnionWith(eventTypes.Select(x => x.FullName));
        }

        public bool DoesEventPassFilter(Event @event, string streamId)
        {
            if (!_eventTypes.Contains(@event.GetType()))
                return false;

            if (_streamId != null)
                return _streamId.Id == streamId;

            foreach (var pattern in _streamExpressions)
            {
                if (Regex.IsMatch(streamId, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
      
            return false;
        }

        /// <summary>
        /// This method checks the event type without requiring the event payload to
        /// be deserialized.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="streamId"></param>
        /// <returns></returns>
        public bool DoesEventPassFilter(string eventType, string streamId)
        {
            if (!_eventTypeStrings.Contains(eventType))
                return false;

            if (_streamId != null)
                return _streamId.Id == streamId;

            foreach (var pattern in _streamExpressions)
            {
                if (Regex.IsMatch(streamId, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
