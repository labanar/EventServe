using System;

namespace EventServe
{
    public class StreamIdBuilder
    {
        private string aggregateName;
        private Guid aggregateId;

        private string streamId;

        private StreamIdBuilder() { }

        public static StreamIdBuilder Create()
        {
            return new StreamIdBuilder();
        }


        public StreamIdBuilder WithAggregateType<T>()
            where T: AggregateRoot
        {
            this.aggregateName = typeof(T).Name;
            return this;
        }

        public StreamIdBuilder WithAggregateType(Type t)
        {
            this.aggregateName = t.Name;
            return this;
        }

        public StreamIdBuilder WithAggregateId(Guid id)
        {
            this.aggregateId = id;
            return this;
        }

        public StreamIdBuilder FromAggregateRoot(AggregateRoot aggregate)
        {
            this.aggregateId = aggregate.Id;
            this.aggregateName = aggregate.GetType().Name;
            return this;
        }

        public StreamIdBuilder FromStreamId(string id)
        {
            this.streamId = id;
            return this;
        }

        public string Build()
        {
            if (!string.IsNullOrEmpty(streamId))
                return streamId;
            else if (!string.IsNullOrEmpty(aggregateName) && aggregateId != null)
                return $"{aggregateName.ToUpper()}-{aggregateId}";

            throw new ArgumentException("StreamId or Aggregate must be supplied.");
        }

    }
}
