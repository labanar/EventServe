using System.Collections.Generic;

namespace EventServe
{
    public abstract class EventStreamSubscription
    {
        public abstract string Name { get; }

        public abstract IStream Stream { get; }
    }
}
