using System;

namespace EventServe
{
    public class StreamDeletedException : Exception
    {
        public StreamDeletedException(string streamId)
             : base($"Stream deleted: {streamId}")
        {

        }
    }
}
