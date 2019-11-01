using System;

namespace EventServe
{
    public class StreamBuilder
    {
        private string aggregateName;
        private Guid aggregateId;

        private string streamId;

        private StreamBuilder() { }

        public static StreamBuilder Create()
        {
            return new StreamBuilder();
        }


        public StreamBuilder WithAggregateType<T>()
            where T: AggregateRoot
        {
            this.aggregateName = typeof(T).Name;
            return this;
        }

        public StreamBuilder WithAggregateId(Guid id)
        {
            this.aggregateId = id;
            return this;
        }

        public StreamBuilder FromAggregateRoot(AggregateRoot aggregate)
        {
            this.aggregateId = aggregate.Id;
            this.aggregateName = aggregate.GetType().Name;
            return this;
        }

        public StreamBuilder FromStreamId(string id)
        {
            this.streamId = id;
            return this;
        }

        public Stream Build()
        {
            if (!string.IsNullOrEmpty(streamId))
                return new Stream(streamId);
            else if (!string.IsNullOrEmpty(aggregateName) && aggregateId != default && aggregateId != null)
                return new Stream($"{aggregateName.ToUpper()}-{aggregateId}");

            throw new ArgumentException("StreamId or Aggregate must be supplied.");
        }

    }
}
