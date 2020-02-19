using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Projections
{
    public interface IProjectionStateRepository
    {
        Task<T> GetProjectionState<T>() where T : Projection;
        Task<T> SetProjectionState<T>(T state) where T : Projection;
    }
}
