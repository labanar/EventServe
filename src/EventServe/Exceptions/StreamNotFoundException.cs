using System;

namespace EventServe
{
    public class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(string streamId)
            : base($"Stream not not found: {streamId}")
        {

        }
    }
}
