using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IProjectionEventHandler<T> where T: IProjection
    {
        Task<T> HandleEvent(T projection, Event @event);
    }
}
