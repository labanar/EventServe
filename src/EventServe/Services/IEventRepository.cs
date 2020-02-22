using System;
using System.Threading.Tasks;

namespace EventServe.Services
{
    public interface IEventRepository<T>
       where T : AggregateRoot
    {
        Task<T> GetById(Guid id);
        Task<long> SaveAsync(AggregateRoot aggregate);
    }
}
