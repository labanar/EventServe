using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.EventStore.Projections
{
    public interface IEventStoreProjectionStateProvider
    {
        Task<string> GetProjectionState(string projectionName);
        Task<string> GetProjectionState(string projectionName, string partitionId);
    }
}
