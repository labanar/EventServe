﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventStreamReader
    {
        IAsyncEnumerable<Event> ReadAllEventsFromStreamAsync(string stream);
    }
}
