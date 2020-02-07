using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EventServe
{
    public class StreamId : IEquatable<StreamId>
    {
        public static StreamId All => new StreamId(Constants.StreamIds.ALL);

        public string Id { get;}

        public StreamId(string streamId)
        {
            Id = streamId;
        }

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


    
}
