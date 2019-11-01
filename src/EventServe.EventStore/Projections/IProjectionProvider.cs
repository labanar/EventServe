using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.EventStore.Projections
{
    public interface IProjectionProvider<T>
    where T : Projection
    {
        Task<T> GetState(bool skipCacheRetrieval = false);
        Task<T> GetPartitionState(string partitionKey, bool skipCacheRetrieval = false);
    }

}
