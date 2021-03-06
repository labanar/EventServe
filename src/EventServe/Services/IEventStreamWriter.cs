﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventStreamWriter
    {
        Task AppendEventToStream(string stream, Event @event);
        Task AppendEventToStream(string stream, Event @event, long? expectedVersion);
        Task AppendEventsToStream(string stream, List<Event> events);
        Task AppendEventsToStream(string stream, List<Event> events, long? expectedVersion);
    }
}
