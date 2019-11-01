using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.EventStore.Projections
{
    public interface IProjectionCache<T>
        where T : Projection, new()
    {
        bool TryGetValue(string partitionKey, out T projection);

        void Set(string partitionKey, T projection);
    }
}
