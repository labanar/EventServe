using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EventServe
{
    public class StreamPosition : IEquatable<StreamPosition>
    {
        public int Position => _position;

        private int _position = -1;


        private StreamPosition(int position) { }

        public static StreamPosition StartOfStream()
        {
            return new StreamPosition(0);
        }

        public static StreamPosition EndOfStream()
        {
            return new StreamPosition(-1);
        }

        public void SetPostionToEnd()
        {
            _position = -1;
        }

        public void SetPositionToBeginning()
        {
            _position = 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StreamPosition);
        }

        public bool Equals([AllowNull] StreamPosition other)
        {
            return other != null &&
                   _position == other._position;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_position);
        }

        public static bool operator ==(StreamPosition left, StreamPosition right)
        {
            return EqualityComparer<StreamPosition>.Default.Equals(left, right);
        }

        public static bool operator !=(StreamPosition left, StreamPosition right)
        {
            return !(left == right);
        }
    }
}
