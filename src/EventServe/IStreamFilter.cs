using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe
{
    public interface IStreamFilter
    {
        public StreamId SubscribedStreamId { get; }
        public Type AggregateType { get; }

        bool DoesEventPassFilter(Event @event, string streamId);

        bool DoesEventPassFilter(string eventType, string streamId);
    }
}
