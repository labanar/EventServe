using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventStreamSubscription
    {
        Task Start();

        Task Stop();

        Task Reset();
    }
}
